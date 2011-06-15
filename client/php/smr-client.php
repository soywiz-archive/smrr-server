<?php

class SmrPacketType {
	const Ping               = 0;
	const ListItems          = 1;
	const SetUser            = 2;
	const LocateUserPosition = 3;
	const SetUsers           = 4;
	
	static public function toString($v) {
		static $lookup;
		if (!isset($lookup)) {
			$class = new ReflectionClass(__CLASS__);
			$lookup = array_flip($class->getConstants());
		}
		return $lookup[$v];
	}
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

class SmrClient {
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
		$this->setUserBufferFlush();
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
		
		//printf("%.6f\n", $end - $start);
		
		return $response;
	}
	
	public function ping() {
		return $this->sendPacket(SmrPacketType::Ping);
	}
	
	//const MAX_SET_USERS = 4000; // pow(2, 16) / (4 * 4)
	//const MAX_SET_USERS = 4096; // pow(2, 16) / (4 * 4)
	const MAX_SET_USERS = 4095; // pow(2, 16) / (4 * 4)
	protected $setUsers = array();
	
	public function setUser($userId, $scoreIndex, $scoreTimestamp, $scoreValue) {
		$result = $this->sendPacket(
			SmrPacketType::SetUser,
			pack('V*', $userId, $scoreIndex, $scoreTimestamp, $scoreValue)
		);
		//print_r($result);
		return $result;
	}
	
	public function setUsers($infos) {
		assert(count($this->setUsers) <= self::MAX_SET_USERS);
		$data = '';
		foreach ($infos as $info) {
			//$data .= pack('V*', $userId, $scoreIndex, $scoreTimestamp, $scoreValue);
			$data .= pack('V*', $info[0], $info[1], $info[2], $info[3]);
			//if (strlen($data) > )
		}
		$result = $this->sendPacket(SmrPacketType::SetUsers, $data);
	}

	public function setUserBuffer($userId, $scoreIndex, $scoreTimestamp, $scoreValue) {
		$this->setUsers[] = array($userId, $scoreIndex, $scoreTimestamp, $scoreValue);
		if (count($this->setUsers) >= self::MAX_SET_USERS) {
			$this->setUserBufferFlush();
		}
	}
	
	public function setUserBufferFlush() {
		if (empty($this->setUsers)) return;

		$this->setUsers($this->setUsers);
		$this->setUsers = array();
	}

	public function locateUserPosition($userId, $scoreIndex) {
		$this->setUserBufferFlush();
		
		$result = $this->sendPacket(
			SmrPacketType::LocateUserPosition,
			pack('V*', $userId, $scoreIndex)
		);
		//print_r($result); return $result;
		list(,$position) = unpack('V', $result->data);
		return $position;
	}

	public function listItems($scoreIndex, $offset, $count) {
		$this->setUserBufferFlush();
	
		$result = $this->sendPacket(
			SmrPacketType::ListItems,
			pack('V*', $scoreIndex, $offset, $count)
		);
		//print_r($result); return $result;
		
		$entries = array();
		
		$data = $result->data;

		while (strlen($data)) {
			$entry = array_combine(array('position', 'userId', 'score', 'timestamp'), array_values(unpack('V4', $data)));
			$data = substr($data, 4 * 4);
			$entries[] = $entry;
		}

		return $entries;
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
