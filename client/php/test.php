<?php

require_once(__DIR__ . '/smr-client.php');

$SmrClient = new SmrClient();
$SmrClient->connect('127.0.0.1', 9777);
$time = time();

//$SmrClient->removeAllElements(0);

printf("Version: %s\n", $SmrClient->getVersion());

$STRESS = true;
//$STRESS = false;

if ($STRESS) {
	$NumberOfIndexes = 100;
	$NumberOfEntriesPerIndexCapped = 10000;
	$NumberOfEntriesToAddPerIndex  = 100000;
} else {
	$NumberOfIndexes = 1;
	$NumberOfEntriesPerIndexCapped = 10000;
	$NumberOfEntriesToAddPerIndex  = 10;
}

for ($m = 0; $m < $NumberOfIndexes; $m++) {
	$Index = $SmrClient->getRankingIdByName('-Index@' . $m . ':' . $NumberOfEntriesPerIndexCapped);

	printf("Index: %d\n", $Index);

	//var_dump($SmrClient->setRanking(0, SmrClientDirection::Ascending, 10000000));

	$start = microtime(true);
	{
		for ($n = 0; $n < $NumberOfEntriesToAddPerIndex; $n++) {
		//for ($n = 0; $n < 1000; $n++) {
		//for ($n = 0; $n < 100; $n++) {
		//for ($n = 0; $n < 20; $n++) {
			$SmrClient->setElementBuffer($Index, $n, mt_rand(0, 500), $time + mt_rand(-50, 4));
		}

		$SmrClient->setElementBuffer($Index, 1000, 200, $time);
		$SmrClient->setElementBuffer($Index, 1001, 300, $time);
		$SmrClient->setElementBuffer($Index, 1000, 300, $time + 1);
		$SmrClient->setElementBufferFlush($Index);
	}
	$end = microtime(true);
	printf("    %f\n", $end - $start);
	//$SmrClient->setElementBufferFlush();

	/*
	printf("Position(1000):%d\n", $pos_1000 = $SmrClient->getElementOffset($Index, 1000));
	print_r($SmrClient->listElements($Index, $pos_1000, 3));

	printf("Position(1001):%d\n", $pos_1001 = $SmrClient->getElementOffset($Index, 1001));
	print_r($SmrClient->listElements($Index, $pos_1001, 3));

	printf("Position(0)\n");
	print_r($SmrClient->listElements($Index, 0, 4));

	printf("Position(997)\n");
	print_r($SmrClient->listElements($Index, 997, 10));
	//print_r($SmrClient->listItems(0, 20, 20));

	print_r($SmrClient->getRankingInfo($Index));
	*/
}


/*
while (true) {
	//echo "[1]";
	$SmrClient->sendPacket(SmrPacketType::Ping);
	//echo "[2]";
	$SmrClient->recvPacket();
	//echo "[3]";
	//$SmrClient->sendPacket(SmrPacketType::Ping);
	//$SmrClient->recvPacket();
}
*/