<?php

class SmrPacketType {
	////////////////////////////////
	/// Misc ///////////////////////
	////////////////////////////////
	const Ping               = 0x00;
	const GetVersion         = 0x01;
	const GetServerInfo      = 0x02;
	////////////////////////////////
	/// Rankings ///////////////////
	////////////////////////////////
	const GetRankingIdByName = 0x10;
	const GetRankingInfo     = 0x11;
	const GetRankingNameById = 0x12;
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

abstract class SmrClientBase {
	public $f;
	private $packetsSended = 0;
	private $packetsSent = 0;
	private $packetsReceived = 0;

	public function __construct() {
	}

	public function __destruct() {
		$this->close();
	}

	public function connect($ip, $port, $timeout = -1) {
		if ($timeout == -1) {
			$timeout = ini_get('default_socket_timeout');
		}
		$this->f = fsockopen($ip, $port, $errno, $errstr, $timeout);
		$this->packetsSended = 0;
		$this->packetsSent = 0;
		if (!$this->f) throw(new Exception("Can't connect to {$ip}:{$port}"));
	}

	public function close() {
		fclose($this->f);
		$this->f = null;
	}

	public function _sendPacket($type, $data = '') {
		if ($this->f == null) throw(new Exception("Must connect before sending any packet."));
		$data_len = strlen($data);
		if ($data_len > 65535) throw(new Exception("Packet is too big '" . $data_len . "'"));
		/*
		fwrite($this->f, pack('v', strlen($data)));
		fwrite($this->f, pack('c', $type));
		fwrite($this->f, $data);
		*/
		fwrite($this->f, pack('v', strlen($data)) . pack('c', $type) . $data);

		$this->packetsSent++;
	}

	public function _recvAllPacketsToSkip() {
		while ($this->packetsReceived < $this->packetsSent) {
			$response = $this->_recvPacket();
		}
	}

	public function _recvPacket() {
		$v = fread($this->f, 3);
		
		if (strlen($v) < 3) throw(new Exception("Error receiving a SmrPacket Expected minimum length 3 and obtained " . strlen($v) . '.'));
		list(,$packetSize) = unpack('v', substr($v, 0, 2));
		list(,$SmrPacketType) = unpack('c', substr($v, 2, 1));
		//echo "[@1:{$packetSize}]";
		//list(,$SmrPacketType) = unpack('c', fread($this->f, 1));
		//echo "[@2:{$SmrPacketType}]";
		$packetData = ($packetSize > 0) ? fread($this->f, $packetSize) : '';
		//echo "[@3:{$packetData}]";

		$this->packetsReceived++;
		//printf("    RECV(recv:%d/sent:%d)\n", $this->packetsReceived, $this->packetsSent);

		return new SmrPacket($SmrPacketType, $packetData);
	}

	public function sendPacket($type, $data = '') {
		$start = microtime(true);
		{
			$this->_recvAllPacketsToSkip();
			$this->_sendPacket($type, $data);
			$response = $this->_recvPacket();
			//echo "Recv ({$response->type})!\n";
		}
		$end = microtime(true);

		if ($response->type != $type) {
			print_r(stream_get_meta_data($this->f));
			throw(new Exception("Mismatch response packet type (Response({$response->type}) != Expected({$type}))"));
		}

		//printf("%.6f\n", $end - $start);

		return $response;
	}
}

class SmrClient extends SmrClientBase {
	protected $bufferSetElements = array();
	protected $cachedRankingIdsByNames = array();
	
	public function __destruct() {
		$this->cachedRankingIdsByNames = array();
	}

	public function ping() {
		return $this->sendPacket(SmrPacketType::Ping);
	}

	public function getVersion() {
		$result = $this->sendPacket(SmrPacketType::GetVersion);
		list(,$major,$minor,$revision,$patch) = unpack('c*', $result->data);
		return "{$major}.{$minor}.{$revision}.{$patch}";
	}

	public function getServerInfo() {
		static $keys = array(
			'indexCount',
			'totalNumberOfElements',
			'currentPrivateMemory',
			'currentVirtualMemory',
			'peakVirtualMemory',
		);
	
		$result = $this->sendPacket(SmrPacketType::GetServerInfo);
		$parts = array_values(unpack('v*', $result->data));
		$values = array();
		for ($n = 0; $n < count($parts); $n += 4) {
			$value = 0;
			for ($m = 3; $m >= 0; $m--) {
				$value *= 0x10000;
				$value += $parts[$n + $m];
			}
			$values[] = $value;
		}
		return array_combine($keys, $values);
	}

	public function _getRankingIndexFromName($rankingName) {
		if (is_int($rankingName)) {
			return $rankingName;
		}
		$cache = &$this->cachedRankingIdsByNames[$rankingName];
		if (!isset($cache)) $cache = $this->getRankingIdByName($rankingName);
		return $cache;
	}
	
	public function getRankingNameById($rankingId) {
		$result = $this->sendPacket(SmrPacketType::GetRankingNameById, pack('V', $rankingId));
		return $result->data;
	}

	public function getRankingIdByName($rankingName) {
		$result = $this->sendPacket(SmrPacketType::GetRankingIdByName, "{$rankingName}\0");
		list(,$indexId) = unpack('V*', $result->data);
		return $indexId;
	}

	public function getRankingInfoAndName($rankingName) {
		$info = $this->getRankingInfo($rankingName);
		return $info + array('name' => $this->getRankingNameById($info['id']));
		//return $info;
	}

	public function getRankingInfo($rankingName) {
		$rankingIndex = $this->_getRankingIndexFromName($rankingName);
		$result = $this->sendPacket(SmrPacketType::GetRankingInfo, pack('V*', $rankingIndex));
		$info = array();
		list(,$info['result'], $info['length'], $info['direction'], $info['topScore'], $info['bottomScore'], $info['maxElements'], $info['treeHeight']) = unpack('V*', $result->data);
		if ($info['result'] != 0) return null;
		//if ($info['result'] != 0) throw(new Exception("Error in getRankingInfo"));
		return array('id' => $rankingIndex) + $info;
	}

	public function setElementBuffer($rankingName, $elementId, $score, $timestamp) {
		$rankingIndex = $this->_getRankingIndexFromName($rankingName);
		$buffer = &$this->bufferSetElements[$rankingIndex];
		if (!isset($buffer)) $buffer = new SmrClient_RankingBuffer($this, $rankingIndex);
		return $buffer->setElementBuffer($elementId, $score, $timestamp);
	}

	public function setElements($infos) {
		throw(new Exception("Not implemented"));
	}

	public function getElementOffset($rankingName, $elementId) {
		$rankingIndex = $this->_getRankingIndexFromName($rankingName);
		$this->_setElementBufferFlush($rankingIndex);

		$result = $this->sendPacket(
			SmrPacketType::GetElementOffset,
			pack('V*', $rankingIndex, $elementId)
		);
		//print_r($result); return $result;
		list(,$position) = unpack('V', $result->data);
		return $position;
	}

	public function listElements($rankingName, $offset, $count) {
		$rankingIndex = $this->_getRankingIndexFromName($rankingName);
		$this->_setElementBufferFlush($rankingIndex);

		$result = $this->sendPacket(
			SmrPacketType::ListElements,
			pack('V*', $rankingIndex, $offset, $count)
		);
		//print_r($result); return $result;

		$entries = array();

		$data = $result->data;
		
		//echo strlen($data) . "\n";

		while (strlen($data)) {
			$entry = array_combine(array('position', 'elementId', 'score', 'timestamp'), array_values(unpack('V4', substr($data, 0, 4 * 4))));
			$data = substr($data, 4 * 4);
			$entries[] = $entry;
		}

		return $entries;
	}

	public function setElementBufferFlush($rankingName) {
		$rankingIndex = $this->_getRankingIndexFromName($rankingName);
		$this->_setElementBufferFlush($rankingIndex);
	}

	protected function _setElementBufferFlush($rankingIndex) {
		$buffer = &$this->bufferSetElements[$rankingIndex];
		if (isset($buffer)) {
			$buffer->setElementBufferFlush();
		}
	}

	public function removeElements() {
		throw(new Exception("Not implemented"));
	}

	public function removeAllElements($rankingName) {
		$rankingIndex = $this->_getRankingIndexFromName($rankingName);
		$result = $this->sendPacket(SmrPacketType::RemoveAllElements, pack('V', $rankingIndex));
		list(, $result, $removedCount) = unpack('V2', $result->data);
		if ($result != 0) throw(new Exception("Error in removeAllElements"));
		return $removedCount;
	}
}

class SmrClient_RankingBuffer {
	public $smrClient;
	public $rankingId;
	protected $bufferSetElements = array();
	protected $MAX_SET_ELEMENTS;

	public function __construct(SmrClientBase $smrClient, $rankingId) {
		$this->smrClient = $smrClient;
		$this->rankingId = $rankingId;
		$this->MAX_SET_ELEMENTS = floor((pow(2, 16) - 4) / (4 * 3)) - 1;
	}

	public function __destruct() {
		$this->close();
	}

	public function close() {
		$this->setElementBufferFlush();
	}

	public function setElementBuffer($elementId, $score, $timestamp) {
		$this->bufferSetElements[] = pack('V*', $elementId, $score, $timestamp);
		if (count($this->bufferSetElements) >= $this->MAX_SET_ELEMENTS) {
			$this->setElementBufferFlush();
		}
	}

	public function setElementBufferFlush() {
		if (empty($this->bufferSetElements)) return;

		$result = $this->_setElements($this->bufferSetElements);
		$this->bufferSetElements = array();
		return $result;
	}

	protected function _setElements($rawInfos) {
		assert(count($rawInfos) <= $this->MAX_SET_ELEMENTS);
		if (count($rawInfos)) {
			$this->smrClient->_sendPacket(SmrPacketType::SetElements, pack('V', $this->rankingId) . implode('', $rawInfos));
			return true;
		} else {
			return false;
		}
	}
}
