<?php 
include 'database.php';
//$query = "SELECT * FROM users";
$name = $_GET['fname'];
$lname = $_GET['lname'];
$email = $_GET['email'];
$pw = $_GET['password'];
$bday = $_GET['bday'];

$query = "INSERT INTO `users` (`id`, `first_name`, `last_name`, `email`, `password`, `birthday`) VALUES (NULL, '$name', '$lname', '$email', '$pw', '$bday')";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

?>