<?php

require_once(__DIR__ . '/smr-client.php');

$SmrClient = new SmrClient();
$SmrClient->connect('127.0.0.1', 9777);
$time = time();

//for ($n = 0; $n < 100000; $n++) {
for ($n = 0; $n < 1000; $n++) {
//for ($n = 0; $n < 100; $n++) {
//for ($n = 0; $n < 20; $n++) {
	$SmrClient->setUserBuffer($n, 0, $time + mt_rand(-50, 4), mt_rand(0, 500));
}

$SmrClient->setUserBuffer(1000, 0, $time, 200);
$SmrClient->setUserBuffer(1001, 0, $time, 300);
$SmrClient->setUserBuffer(1000, 0, $time + 1, 300);
//$SmrClient->setUserBufferFlush();

printf("Position(1000):%d\n", $pos_1000 = $SmrClient->locateUserPosition(1000, 0));
print_r($SmrClient->listItems(0, $pos_1000, 3));

printf("Position(1001):%d\n", $pos_1001 = $SmrClient->locateUserPosition(1001, 0));
print_r($SmrClient->listItems(0, $pos_1001, 3));

printf("Position(0)\n");
print_r($SmrClient->listItems(0, 0, 4));

printf("Position(9997)\n");
print_r($SmrClient->listItems(0, 997, 10));
//print_r($SmrClient->listItems(0, 20, 20));



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