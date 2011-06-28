<?php

class SmrPacketType {
	////////////////////////////////
	/// Misc ///////////////////////
	////////////////////////////////
	const Ping               = 0x00;
	const GetVersion         = 0x01;
	////////////////////////////////
	/// Rankings ///////////////////
	////////////////////////////////
	const SetRanking         = 0x10;
	const GetRankingInfo     = 0x11;
	////////////////////////////////
	/// Elements ///////////////////
	////////////////////////////////
	const SetElements        = 0x20;
	const GetElementOffset   = 0x21;
	const ListElements       = 0x22;
	const RemoveElements     = 0x23;
	const RemoveAllElements  = 0x24;
	////////////////////////////////

	static public function toString($v) {
		static $lookup;
		if (!isset($lookup)) {
			$class = new ReflectionClass(__CLASS__);
			$lookup = array_flip($class->getConstants());
		}
		return $lookup[$v];
	}
}

class SmrClientDirection {
	const Ascending = +1;
	const Descending = -1;
}

class SmrPacket {
	public $type;
	public $typeString;
	public $data;

	public function __construct($type, $data) {
		$this->type = $type;
		$this->typeString = SmrPacketType::toString($type);
		$this->data = $data;

		//echo SmrPacketType::toString($this->type) . "\n";
	}
}

class SmrClientBase {
	public $f;

	public function __construct() {
	}

	public function __destruct() {
		$this->close();
	}

	public function connect($ip, $port) {
		$this->f = fsockopen($ip, $port);
		if (!$this->f) throw(new Exception("Can't connect to {$ip}:{$port}"));
	}

	public function close() {
		$this->setElementBufferFlush();
		fclose($this->f);
		$this->f = null;
	}

	public function sendPacket($type, $data = '') {
		$start = microtime(true);
		{
			fwrite($this->f, pack('v', strlen($data)));
			fwrite($this->f, pack('c', $type));
			fwrite($this->f, $data);

			$response = $this->recvPacket();
		}
		$end = microtime(true);
		
		if ($response->type != $type) throw(new Exception("Mismatch response packet type"));

		//printf("%.6f\n", $end - $start);

		return $response;
	}
	
	public function recvPacket() {
		//echo "[@0:.]";
		list(,$packetSize) = unpack('v', $v = fread($this->f, 2));
		if (strlen($v) < 2) throw(new Exception("Error receiving a SmrPacket"));
		//echo "[@1:{$packetSize}]";
		list(,$SmrPacketType) = unpack('c', fread($this->f, 1));
		//echo "[@2:{$SmrPacketType}]";
		$packetData = ($packetSize > 0) ? fread($this->f, $packetSize) : '';
		//echo "[@3:{$packetData}]";

		return new SmrPacket($SmrPacketType, $packetData);
	}
}

class SmrClient extends SmrClientBase {
	// Buffer
	const MAX_SET_ELEMENTS = 4095; // pow(2, 16) / (4 * 4)
	protected $bufferSetElements = array();

	public function ping() {
		return $this->sendPacket(SmrPacketType::Ping);
	}

	public function getVersion() {
		$result = $this->sendPacket(SmrPacketType::GetVersion);
		list(,$version) = unpack('V', $result->data);
		return $version;
	}
	
	public function setRanking($rankingIndex, $direction, $maxElements) {
		$result = $this->sendPacket(SmrPacketType::SetRanking, pack('V*', $rankingIndex, $direction, $maxElements));
		list(,$result, $removedCount) = unpack('V2', $result->data);
		if ($result != 0) throw(new Exception("Error in setRanking"));
		return $removedCount;
	}
	
	public function getRankingInfo($rankingIndex) {
		$result = $this->sendPacket(SmrPacketType::GetRankingInfo, pack('V*', $rankingIndex));
		$info = array();
		list(,$info['result'], $info['length'], $info['direction'], $info['topScore'], $info['bottomScore'], $info['maxElements'], $info['treeHeight']) = unpack('V*', $result->data);
		if ($info['result'] != 0) return null;
		//if ($info['result'] != 0) throw(new Exception("Error in getRankingInfo"));
		return $info;
	}

	public function setElementBuffer($rankingIndex, $elementId, $score, $timestamp) {
		$this->bufferSetElements[] = pack('V*', $rankingIndex, $elementId, $score, $timestamp);
		if (count($this->bufferSetElements) >= self::MAX_SET_ELEMENTS) {
			$this->setElementBufferFlush();
		}
	}

	public function setElements($infos) {
		throw(new Exception("Not implemented"));
	}

	protected function _setElements($rawInfos) {
		assert(count($rawInfos) <= self::MAX_SET_ELEMENTS);
		if (count($rawInfos)) {
			return $this->sendPacket(SmrPacketType::SetElements, implode('', $rawInfos));
		} else {
			return false;
		}
	}

	public function setElementBufferFlush() {
		if (empty($this->bufferSetElements)) return;

		$result = $this->_setElements($this->bufferSetElements);
		$this->bufferSetElements = array();
		return $result;
	}

	public function getElementOffset($rankingIndex, $elementId) {
		$this->setElementBufferFlush();

		$result = $this->sendPacket(
			SmrPacketType::GetElementOffset,
			pack('V*', $rankingIndex, $elementId)
		);
		//print_r($result); return $result;
		list(,$position) = unpack('V', $result->data);
		return $position;
	}

	public function listElements($rankingIndex, $offset, $count) {
		$this->setElementBufferFlush();

		$result = $this->sendPacket(
			SmrPacketType::ListElements,
			pack('V*', $rankingIndex, $offset, $count)
		);
		//print_r($result); return $result;

		$entries = array();

		$data = $result->data;

		while (strlen($data)) {
			$entry = array_combine(array('position', 'elementId', 'score', 'timestamp'), array_values(unpack('V4', $data)));
			$data = substr($data, 4 * 4);
			$entries[] = $entry;
		}

		return $entries;
	}

	public function removeElements() {
		throw(new Exception("Not implemented"));
	}
	
	public function removeAllElements($rankingIndex) {
		$result = $this->sendPacket(SmrPacketType::RemoveAllElements, pack('V', $rankingIndex));
		list(, $result, $removedCount) = unpack('V2', $result->data);
		if ($result != 0) throw(new Exception("Error in removeAllElements"));
		return $removedCount;
	}
}
