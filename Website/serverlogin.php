<?php 
include 'database.php';



$id = $_GET['id'];
$pw = $_GET['password'];

//https://studenthome.hku.nl/~niels.poelder/Kernmodule4/serverlogin.php?id=1&password=Kipje

$query = "SELECT * FROM servers WHERE `id`='$id' AND `password`='$pw' LIMIT 1";

if (!($result = $mysqli->query($query)))
showerror($mysqli->errno,$mysqli->error);

$row = $result->fetch_assoc();

if(!$row["name"] == "") 
{
    session_start();
    do 
    {
        echo json_encode($row);
    }
    while ($row = $result->fetch_assoc());
    
    //echo session_id();
}
else
{
    echo "0";
}





//https://studenthome.hku.nl/~niels.poelder/Kernmodule4/serverlogin.php?id=1&password=Kipj

?>