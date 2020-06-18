<?php 
include 'database.php';

session_start();

//$userid = $_GET['user_id'];


//$_SESSION['speler']=$userid;




//https://studenthome.hku.nl/~niels.poelder/Kernmodule4/login.php?game_id=1&user_id=5&score=1&date=2020-05-18&email=gerard@gmail.com&password=Renvoorjeleven

//$query = "SELECT * FROM users";
//$game_id = $_GET['game_id'];
//$score = $_GET['score'];
//$date = $_GET['date'];

//$query = "INSERT INTO `scores` (`id`, `game_id`, `user_id`, `score`, `date_time`) VALUES (NULL, '$game_id', $userid, '$score', '$date')";

//if (!($result = $mysqli->query($query)))
//showerror($mysqli->errno,$mysqli->error);

$email = $_GET['email'];
$pw = $_GET['password'];


$query = "SELECT * FROM users WHERE `email`='$email' AND `password`='$pw' LIMIT 1";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

//https://studenthome.hku.nl/~niels.poelder/Kernmodule4/login.php?email=kat@kaas.nl&password=Kipje

if(!$row["id"] == "") 
{
    echo json_encode($row);
    echo "<br>";
    //do 
    //{
    //echo $row["id"] . $row["email"] . "<br>";
    //$_SESSION['speler'] = $row["id"];
    //} 
    //while ($row = $result->fetch_assoc());
    //echo "<BR> ". $_SESSION['speler'];
//    echo json_encode($row);
}
else
{
    echo "0";
}

?>