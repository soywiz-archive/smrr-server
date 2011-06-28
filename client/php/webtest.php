<?php

class DatabaseTableOperation {

}

class DatabaseTableQuery {
	/**
	 * @var DatabaseTable
	 */
	protected $table;

	protected $query = array();

	public function __construct(DatabaseTable $table) {
		$this->table = $table;
	}

	public function equals($name, $value) {
		$this->query[$name] = $value;
	}

	public function getWhereString() {
		return '1=1';
	}

	public function getQueryString() {
		$sql = '';
		$sql .= "SELECT * FROM " . $this->table->db->quote($this->table->tableName);
		$sql .= ' WHERE ' . $this->getWhereString();
		$sql .= '';
		return $sql;
	}

	public function exec() {
		$stm = $this->table->db->prepare($this->getQueryString());
		$stm->setFetchMode(PDO::FETCH_ASSOC);
		$stm->execute(array());
		return $stm;
	}
}

class DatabaseTable {
	public $db;
	public $tableName;

	public function __construct(Database $db, $tableName) {
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

	public function delete() {
		$sql = 'DELETE FROM ' . $this->db->quote($this->tableName) . ';';
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

	public function find($query) {
		$databaseTableQuery = new DatabaseTableQuery($this);
		return $databaseTableQuery->find($query);
	}
}

class Database extends PDO {
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
}

$startTime = microtime(true);
{
	$db = new Database(__DIR__ . '/db.sqlite3');
	$db->test->create(array('a', 'b', 'c'));
	$db->test->delete();
	$db->test->insert(array('a' => 1, 'b' => 2, 'c' => 3));
	$query = $db->test->find(array('a' => 1));
	foreach ($query->exec() as $row) {
		print_r($row);
	}
}
$endTime = microtime(true);
printf("%.5f\n", $endTime - $startTime);

//$db->find('test', array('a' => 1));
