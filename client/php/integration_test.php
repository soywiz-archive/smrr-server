<?php

error_reporting(E_ALL);
set_time_limit(0);

//proc_open( string $cmd , array $descriptorspec , array &$pipes [, string $cwd [, array $env [, array $other_options ]]] )

class SimpleMassiveServer {
	protected $ip;
	protected $port;
	protected $executable;
	static protected $executable_test;
	protected $pipes = array();

	public function __construct($port = 9999, $ip = '127.0.0.1') {
		$this->ip = $ip;
		$this->port = $port;
		//$this->executable = realpath(__DIR__ . '/../../server-cs/SimpleMassiveRealtimeRankingServer.exe');
		$this->executable = realpath(__DIR__ . '/../../server-cs/SimpleMassiveRealtimeRankingServer/bin/Debug/SimpleMassiveRealtimeRankingServer.exe');
		static::$executable_test = "smrr_integration_test.exe";
		//echo "{$this->executable}\n";
	}
	
	public function __destructor() {
		$this->stop();
	}
	
	public function start() {
		static::clean();
		
		@copy($this->executable, static::$executable_test);
		@copy(dirname($this->executable) . '/CSharpUtils.dll', dirname(static::$executable_test) . '/CSharpUtils.dll');
		
		$this->proc = proc_open(
			static::$executable_test . " -i={$this->ip} -p={$this->port}",
			array(
				0 => array('pipe', "r"),  // stdin es una tubería usada por el hijo para lectura
				1 => array('pipe', "w"),  // stdout es una tubería usada por el hijo para escritura
				2 => array('pipe', "w"),  // stderr es una tubería usada por el hijo para escritura
			),
			$this->pipes
		);
		
		stream_set_blocking($this->pipes[1], false);
		stream_set_blocking($this->pipes[2], false);
		
		register_shutdown_function(array($this, 'clean_and_flush'));
	}
	
	public function stop() {
		if ($this->proc != null) {
			//stream_set_blocking($this->pipes[1], false); echo fread($this->pipes[1], 100000);
			//stream_set_blocking($this->pipes[2], false); echo fread($this->pipes[2], 100000);
			static::clean();
			proc_close($this->proc);
		}
	}
	
	public function clean_and_flush() {
		//print_r(stream_get_meta_data($this->pipes[1]));
		//print_r(stream_get_meta_data($this->pipes[2]));
		static::clean();

		@$stdout_data = fread($this->pipes[1], 10240);
		@$stderr_data = fread($this->pipes[2], 10240);

		if ($stdout_data || $stderr_data) {
			if ($stdout_data) {
				echo "---------------------------------------------------\n";
				echo "{$stdout_data}\n";
			}
			if ($stderr_data) {
				echo "---------------------------------------------------\n";
				echo "{$stderr_data}\n";
			}
			echo "---------------------------------------------------\n";
		}
	}

	static public function clean() {
		$a = error_get_last(); 
		
		//echo "Cleaning up...";
		@shell_exec("taskkill /F /IM " . basename(static::$executable_test) . " 2> NUL");
	}
}

//echo fread($pipes[1], 1000);

require_once(__DIR__ . '/smr-client.php');

function assertAreEqual($expected, $actual) {
	$backtrace = debug_backtrace();
	$backrow = $backtrace[0];
	list($file, $line) = array($backrow['file'], $backrow['line']);
	if ($expected != $actual) {
		$lines = file($file);
		print_r($expected);
		echo " != \n";
		print_r($actual);
		throw(new Exception("Assert failed on '{$file}:{$line}' :: " . trim($lines[$line - 1])));
	}
}

//$port = 9999;
$port = 11111;
$server = new SimpleMassiveServer($port);
$server->start();
{
	$client = new SmrClient();
	$client->connect('127.0.0.1', $port);
	$index = $client->getRankingIdByName('-testIndex:99');
	assertAreEqual(
		array(
			'id' => 0,
			'treeHeight' => -1,
			'maxElements' => -1,
			'bottomScore' => 0,
			'topScore' => 0,
			'direction' => -1,
			'length' => 0,
			'result' => 0,
			'name' => '-testIndex:99',
		),
		$client->getRankingInfoAndName($index)
	);
	$time = 1322747689;
	$client->setElementBuffer($index, $elementId = 1000, $score = 300, $timestamp = $time);
	$client->setElementBuffer($index, $elementId = 1001, $score = 400, $timestamp = $time);
	$client->setElementBuffer($index, $elementId = 1001, $score = 350, $timestamp = $time + 1);
	$client->setElementBuffer($index, $elementId = 1000, $score = 320, $timestamp = $time + 1);
	$client->setElementBuffer($index, $elementId = 1000, $score = 300, $timestamp = $time + 2);
	$client->setElementBuffer($index, $elementId = 1002, $score = 200, $timestamp = $time + 1);
	$client->setElementBuffer($index, $elementId = 1003, $score = 100, $timestamp = $time + 1);
	
	$client->setElementBuffer($index, $elementId = 1000, $score = 300, $timestamp = $time + 2);
	
	//$client->setElementBuffer($index, $elementId = 1000, $score = 10000, $timestamp = $time + 100);
	
	//print_r($client->listElements($index, $offset = 0, $count = 10)); exit;
	
	$client->setElementBufferFlush($index);
	
	assertAreEqual(
		array(
			'id' => 0,
			'treeHeight' => -1,
			'maxElements' => -1,
			'bottomScore' => 100,
			'topScore' => 400,
			'direction' => -1,
			'length' => 4,
			'result' => 0,
		),
		$client->getRankingInfo($index)
	);
	assertAreEqual(
		array (
			array (
				'position' => 0,
				'elementId' => 1001,
				'score' => 400,
				'timestamp' => 1322747689,
			),
			array (
				'position' => 1,
				'elementId' => 1000,
				'score' => 320,
				'timestamp' => 1322747690,
			),
			array (
				'position' => 2,
				'elementId' => 1002,
				'score' => 200,
				'timestamp' => 1322747690,
			),
			array (
				'position' => 3,
				'elementId' => 1003,
				'score' => 100,
				'timestamp' => 1322747690,
			),
		),
		$client->listElements($index, $offset = 0, $count = 10)
	);
	assertAreEqual(
		array(
			array (
				'position' => 1,
				'elementId' => 1000,
				'score' => 320,
				'timestamp' => 1322747690,
			),
			array (
				'position' => 2,
				'elementId' => 1002,
				'score' => 200,
				'timestamp' => 1322747690,
			),
		),
		$client->listElements($index, $offset = 1, $count = 2)
	);
	assertAreEqual(
		array(
		),
		$client->listElements($index, $offset = -1, $count = 10)
	);

	assertAreEqual(
		2,
		$client->getElementOffset($index, 1002)
	);

	assertAreEqual(4, $client->removeAllElements($index), 'removeAllElements');
	
	assertAreEqual(
		array(
		),
		$client->listElements($index, $offset = 0, $count = 10)
	);
	assertAreEqual(
		array (
			'id' => 0,
			'treeHeight' => -1,
			'maxElements' => -1,
			'bottomScore' => 0,
			'topScore' => 0,
			'direction' => -1,
			'length' => 0,
			'result' => 0,
		),
		$client->getRankingInfo($index)
	);
}
$server->stop();
echo "Ok\n";