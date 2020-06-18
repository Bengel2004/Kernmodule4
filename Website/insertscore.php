<?php 
include 'database.php';
//$query = "SELECT * FROM users";
$userid = $_GET['uid'];
$sessionid = $_GET['sid'];
$score = $_GET['score'];

$query = "INSERT INTO `scores` (`id`, `game_id`, `user_id`, `place`, `score`, `date_time`) VALUES (NULL, '$sessionid', '$userid', '1', '$score', NOW())";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);


?>