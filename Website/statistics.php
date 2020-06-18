<?php 
include 'database.php';


//$query = "SELECT * FROM users";
echo "<b>AVG Scores Last Month</b><br>";
//$query = "SELECT `id`, `user_id`, MAX(`score`) as highscore FROM scores WHERE `game_id`=1";
$query = "SELECT s.id, COUNT(*), s.date_time,p.first_name, AVG(s.score) AS 'avg_score' FROM scores s LEFT JOIN users p ON (s.user_id = p.id) WHERE s.date_time>= DATE_FORMAT(CURRENT_DATE - INTERVAL 1 MONTH, '%Y-%m-01') GROUP BY p.id ORDER BY DATE_FORMAT(s.date_time, '%m')";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

do {
    echo json_encode($row) . "<br>";
} 
while ($row = $result->fetch_assoc());


echo "<br><b>Top 5 last month</b><br>";
$query = "SELECT s.id, s.date_time,p.first_name, s.place, s.score FROM scores s LEFT JOIN users p ON (s.user_id = p.id) WHERE s.place=1 AND s.date_time>= DATE_FORMAT(CURRENT_DATE - INTERVAL 1 MONTH, '%Y-%m-01') ORDER BY s.score DESC LIMIT 5";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();


do {
    echo json_encode($row) . "<br>";
} 
while ($row = $result->fetch_assoc());


    
echo "<br><b>Times played 1st place last month</b><br>";
$query = "SELECT COUNT(*) as 'Times played' FROM scores s LEFT JOIN users p ON (s.user_id = p.id) WHERE s.place=1 AND s.date_time>= DATE_FORMAT(CURRENT_DATE - INTERVAL 1 MONTH, '%Y-%m-%d') ORDER BY s.score DESC LIMIT 5";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();


do {
    echo json_encode($row) . "<br>";
} 
while ($row = $result->fetch_assoc());


    
    
echo "<br><b>Top 5 2nd place last month</b><br>";

$query = "SELECT s.id, s.date_time,p.first_name, s.place, s.score FROM scores s LEFT JOIN users p ON (s.user_id = p.id) WHERE s.place=1 AND s.date_time>= DATE_FORMAT(CURRENT_DATE - INTERVAL 1 MONTH, '%Y-%m-01') ORDER BY s.score DESC LIMIT 5";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

do {
    echo json_encode($row) . "<br>";
} 
while ($row = $result->fetch_assoc());

echo "<br><b>Times played 2nd place last month</b><br>";
$query = "SELECT COUNT(*) as 'Times played' FROM scores s LEFT JOIN users p ON (s.user_id = p.id) WHERE s.place=2 AND s.date_time>= DATE_FORMAT(CURRENT_DATE - INTERVAL 1 MONTH, '%Y-%m-%d') ORDER BY s.score DESC LIMIT 5";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();


do {
    echo json_encode($row) . "<br>";
} 
while ($row = $result->fetch_assoc());




//echo json_encode($row);

?>