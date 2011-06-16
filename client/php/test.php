<?php

require_once(__DIR__ . '/smr-client.php');

$SmrClient = new SmrClient();
$SmrClient->connect('127.0.0.1', 9777);
$time = time();

//$SmrClient->removeAllElements(0);

printf("Version: %d\n", $SmrClient->getVersion());

if ($SmrClient->getRankingInfo(0) === null) {
	$SmrClient->setRanking(0, SmrClientDirection::Descending, -1);
}

$SmrClient->setRanking(0, SmrClientDirection::Descending, 1000);

//var_dump($SmrClient->setRanking(0, SmrClientDirection::Ascending, 10000000));

for ($n = 0; $n < 100000; $n++) {
//for ($n = 0; $n < 1000; $n++) {
//for ($n = 0; $n < 100; $n++) {
//for ($n = 0; $n < 20; $n++) {
	$SmrClient->setElementBuffer(0, $n, mt_rand(0, 500), $time + mt_rand(-50, 4));
}

$SmrClient->setElementBuffer(0, 1000, 200, $time);
$SmrClient->setElementBuffer(0, 1001, 300, $time);
$SmrClient->setElementBuffer(0, 1000, 300, $time + 1);
//$SmrClient->setElementBufferFlush();

printf("Position(1000):%d\n", $pos_1000 = $SmrClient->getElementOffset(0, 1000));
print_r($SmrClient->listElements(0, $pos_1000, 3));

printf("Position(1001):%d\n", $pos_1001 = $SmrClient->getElementOffset(0, 1001));
print_r($SmrClient->listElements(0, $pos_1001, 3));

printf("Position(0)\n");
print_r($SmrClient->listElements(0, 0, 4));

printf("Position(997)\n");
print_r($SmrClient->listElements(0, 997, 10));
//print_r($SmrClient->listItems(0, 20, 20));

print_r($SmrClient->getRankingInfo(0));


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