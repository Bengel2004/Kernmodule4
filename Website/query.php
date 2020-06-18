<?php 
include 'database.php';


//$query = "SELECT * FROM users";
$query = "SELECT MAX(`score`), `game_id` FROM scores WHERE `game_id`=5";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

do {
echo $row["game_id"] . $row["MAX(`score`)"] . "<br>";
} while ($row = $result->fetch_assoc());


$query = "SELECT `id`, `user_id`, MAX(`score`) as highscore FROM scores WHERE `game_id`=1";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

echo json_encode($row);

?>