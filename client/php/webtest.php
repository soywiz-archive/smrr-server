<?php

class DatabaseTableQuery implements IteratorAggregate {
	/**
	 * @var DatabaseTable
	 */
	protected $table;

	/**
	 * @var DbOp
	 */
	protected $where;

	protected $items = NULL;

	public function __construct(DatabaseTable $table, DbOp $where) {
		$this->table = $table;
		$this->where = $where;
	}

	public function getQueryString() {
		$sql = '';
		$sql .= "SELECT * FROM " . $this->table->db->quote($this->table->tableName);
		$sql .= ' WHERE ' . $this->where->getCondition($this->table->db);
		$sql .= ';';
		//echo $sql;
		return $sql;
	}

	public function getItems() {
		if ($this->items === NULL) {
			$stm = $this->table->db->prepare($this->getQueryString());
			$stm->setFetchMode(PDO::FETCH_ASSOC);
			$stm->execute(array());
			$this->items = iterator_to_array($stm);
		}
		return $this->items;
	}

	public function getIterator () {
		return new ArrayIterator($this->getItems());
	}

	public function sortByKeyOrder($key, $keysOrder) {
		return sortByKey($this->getItems(), $key, $keysOrder);
	}
}

class DatabaseTable {
	/**
	 * @var Db
	 */
	public $db;

	/**
	 * @var string
	 */
	public $tableName;

	public function __construct(Db $db, $tableName) {
		$this->db = $db;
		$this->tableName = $tableName;
	}

	public function create($fields, $ifNotExists = true) {
		$sql = '';
		$sql .= 'CREATE TABLE';
		if ($ifNotExists) $sql .= ' IF NOT EXISTS';
		$sql .= ' ' . $this->db->quote($this->tableName);
		$sql .= ' (' . implode(', ', array_map(array($this->db, 'quote'), $fields)) . ')';
		$sql .= ';';
		return $this->db->query($sql);
	}

	public function delete($where = NULL) {
		//new DatabaseTableQuery($this, $where);
		$sql = 'DELETE FROM ' . $this->db->quote($this->tableName) . ' WHERE ' . db_ONE($where)->getCondition($this->db) . ';';
		//echo "$sql\n";
		$this->db->query($sql);
	}

	public function insert($array) {
		$sql = '';
		$sql .= 'INSERT INTO ' . $this->db->quote($this->tableName);
		$sql .= ' (' . implode(',', array_map(array($this->db, 'quote'), array_keys($array))) . ')';
		$sql .= ' VALUES';
		$sql .= ' (' . implode(',', array_map(array($this->db, 'quote'), array_values($array))) . ')';
		$sql .= ';';
		$this->db->query($sql);
		return $sql;
	}

	public function find($where = NULL) {
		return new DatabaseTableQuery($this, db_ONE($where));
	}
}

class Db extends PDO {
	public function __construct($dbFile) {
		parent::__construct('sqlite:' . $dbFile);
		$this->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
		$this->query('PRAGMA synchronous=OFF;');
	}

	public function getTable($tableName) {
		return new DatabaseTable($this, $tableName);
	}

	public function __get($tableName) {
		return $this->$tableName = $this->getTable($tableName);
	}

	public function quoteId($name) {
		return '"' . str_replace('"', '""', $name) . '"';
	}
}

abstract class DbOp {
	abstract public function getCondition(Db $db);
}

class DbOpUnary extends DbOp {
	/**
	 * @var DbOp
	 */
	public $a;

	/**
	 * @var string
	 */
	public $op;

	public function __construct($a, $op) {
		$this->a  = $a;
		$this->op = $op;
	}

	public function getCondition(Db $db) {
		if ($this->op == 'ONE') {
			if ($this->a == NULL) return '1';
			return $this->a->getCondition($db);
		}
		return $this->op . ' (' . $this->a->getCondition($db) . ')';
	}
}

class DbOpBinary extends DbOp {
	/**
	 * @var DbOp
	 */
	public $a;

	/**
	 * @var DbOp
	 */
	public $b;

	/**
	 * @var string
	 */
	public $op;

	public function __construct($a, $op, $b) {
		$this->a  = $a;
		$this->b  = $b;
		$this->op = $op;
	}

	public function getCondition(Db $db) {
		$ret = '';
		$ret .= $db->quoteId($this->a);
		if (is_array($this->b)) {
			if ($this->op != 'IN') throw(new Exception("Invalid IN"));
			$ret .= ' IN ';
			$ret .= '(' . implode(', ', array_map(array($db, 'quote'), $this->b)) . ')';
		} else {
			$ret .= ' ' . $this->op . ' ';
			$ret .= $db->quote($this->b);
		}
		return $ret;
	}
}

class DbOpList extends DbOp {
	protected $separator;
	protected $items = array();

	public function __construct($separator) {
		$this->separator = $separator;
	}

	public function add($item) {
		$this->items[] = $item;
	}

	public function getCondition(Db $db) {
		if (count($this->items)) {
			$itemStrings = array_map(function($item) use ($db) {
				return $item->getCondition($db);
			}, $this->items);

			$combinedSeparator = ') ' . $this->separator . ' (';

			return '(' . implode(
				$combinedSeparator,
				$itemStrings
			) . ')';
		} else {
			return $this->items[0];
		}
	}
}

function db_AND_OR($op, $args) {
	$dbOp = new DbOpList($op);
	foreach ($args as $arg) $dbOp->add($arg);
	return $dbOp;
}

function db_AND() {
	$args = func_get_args();
	return db_AND_OR('AND', $args);
}

function db_OR() {
	$args = func_get_args();
	return db_AND_OR('OR', $args);
}

function db_EQ ($a, $b) { return new DbOpBinary($a, '=',  $b); }
function db_GT ($a, $b) { return new DbOpBinary($a, '>',  $b); }
function db_GTE($a, $b) { return new DbOpBinary($a, '>=', $b); }
function db_LT ($a, $b) { return new DbOpBinary($a, '<',  $b); }
function db_LTE($a, $b) { return new DbOpBinary($a, '<=', $b); }
function db_IN ($a, $b) { return new DbOpBinary($a, 'IN', $b); }
function db_NOT($a    ) { return new DbOpUnary($a, 'NOT'); }
function db_ONE($a    ) { return new DbOpUnary($a, 'ONE'); }

function sortByKey($items, $keyName, $keys) {
	$itemsByKey = array();
	foreach ($items as $item) {
		if (is_object($item)) {
			$itemsByKey[$item->$keyName] = $item;
		} else {
			$itemsByKey[$item[$keyName]] = $item;
		}
	}
	$itemsSorted = array();
	foreach ($keys as $key) $itemsSorted[] = $itemsByKey[$key];
	return $itemsSorted;
}

$startTime = microtime(true);
{
	$db = new Db(__DIR__ . '/db.sqlite3');
	$db->test->create(array('a', 'b', 'c'));
	$db->test->delete();
	$db->test->insert(array('a' => 1, 'b' => 2, 'c' => 3));
	$db->test->insert(array('a' => 2, 'b' => 3, 'c' => 4));
	//$db->test->delete(db_IN('a', array(1, 2)));
	$query = $db->test->find();
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
