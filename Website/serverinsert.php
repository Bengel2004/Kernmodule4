<?php 
include 'database.php';


if (isset($_GET['session_id'])) {

    
    $sid = htmlspecialchars($_GET['session_id']);
    
    session_id($sid);
    session_start();
    
    
    //https://studenthome.hku.nl/~niels.poelder/Kernmodule4/serverinsert.php?session_id=1&game_id=1&user_id=1&score=1
    if(isset($_SESSION['server_id'], $_SESSION['server_id']))
    {
        
        //$query = "SELECT * FROM users";
        $gameid = $_GET['game_id'];
        $userid = $_GET['user_id'];
        $score = $_GET['score'];

        


        $query = "INSERT INTO `scores` (`id`, `game_id`, `user_id`, `score`, `date_time`) VALUES (NULL, '$gameid', '$userid', '$score', NOW())";
        echo "Score $score for user $userid on Game $gameid has been set!";
            
        if (!($result = $mysqli->query($query)))
        showerror($mysqli->errno,$mysqli->error);

    }
    else
    {   
        echo "ERROR, ID NOT FOUND <br>";
        echo $_SESSION['server_id']. " <br>";
    }

}

?>