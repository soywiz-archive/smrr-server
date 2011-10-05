<?php

require_once(__DIR__ . '/db_orm.php');

class User {
	public $name;
	public $score;

	public function __construct($name, $score) {
		$this->name  = $name;
		$this->score = $score;
	}
}

$startTime = microtime(true);
{
	$db = new Db(__DIR__ . '/db.sqlite3');
	$db->User->create();
	$db->User->insert(new User('test', 1000));
	/*
	$db->test->delete();
	$db->test->insert(array('a' => 2, 'b' => 3, 'c' => 4));
	//$db->test->delete(db_IN('a', array(1, 2)));
	$query = $db->test->find();
	*/
	/*
	$query = $db->test->find(db_AND(
		//db_GTE('a', 1)
		db_IN('a', array(1, 2, 3, 4)),
		db_NOT(
			db_OR(
				db_EQ('b', 2),
				db_EQ('b', 3)
			)
		)
	));
	*/
	$query = $db->User->find();
	echo $query->getQueryString();
	print_r($query->sortByKeyOrder('a', array(1, 2)));
	print_r($query->sortByKeyOrder('a', array(2, 1)));
	/*
	$items = iterator_to_array($query);
	print_r(sortByKey($items, 'a', array(1, 2)));
	print_r(sortByKey($items, 'a', array(2, 1)));
	*/
}
$endTime = microtime(true);
printf("%.5f\n", $endTime - $startTime);

//$db->find('test', array('a' => 1));
