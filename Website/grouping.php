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


//$query = "SELECT `id`, `user_id`, MAX(`score`) as highscore FROM scores WHERE `game_id`=1";
$query = "SELECT s.id, COUNT(*), s.date_time,p.first_name, AVG(s.score) AS 'avg_score' FROM scores s LEFT JOIN users p ON (s.user_id = p.id) GROUP BY p.id ORDER BY s.score DESC";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

do {
    echo $row["id"] ." " . $row["COUNT(*)"] . " " . $row["date_time"] . " " . $row["first_name"] . " " . $row["avg_score"] . "<br>";
} 
while ($row = $result->fetch_assoc());

//echo json_encode($row);

?>