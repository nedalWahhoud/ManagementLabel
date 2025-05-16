<?php
header("Access-Control-Allow-Origin: *");
header("Content-Type: application/json");
require 'vendor/autoload.php';

use Firebase\JWT\JWT;
use Firebase\JWT\Key;

$host_name = '127.0.0.1';
$database = 'storecontrol';
$user_name = 'root';
$password = '';


try {
    $pdo = new PDO("mysql:host=$host_name;dbname=$database;charset=utf8mb4", $user_name, $password);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode([
        "success" => false,
        "message" => "Connection failed: " . $e->getMessage()
    ]);
    exit();
}

// 
$data = json_decode(file_get_contents('php://input'), true);
$username = $data['username'] ?? '';
$password = $data['password'] ?? '';



if (!$username || !$password) {
    http_response_code(400);
    echo json_encode([
        "success" => false,
        "message" => "Username and password are required."
    ]);
    exit();
}



// ✅ التحقق من المستخدم في قاعدة البيانات
$stmt = $pdo->prepare("SELECT * FROM users WHERE username = :username or email = :username  LIMIT 1");
$stmt->bindParam(':username', $username, PDO::PARAM_STR);
$stmt->execute();
$user = $stmt->fetch(PDO::FETCH_ASSOC);



if (!$user || $password !== $user['passwordHash']) { 
    http_response_code(401);
    echo json_encode([
        "success" => false,
        "message" => "Invalid username or password"
    ]);
    exit();
}




// ✅ توليد JWT
$secret_key = bin2hex(random_bytes(32));  // استبدله بمفتاح قوي
$issuer_claim = "http://localhost";
$audience_claim = "http://ِAutoId";
date_default_timezone_set(timezoneId: "Europe/Berlin");
$issuedat_claim = time();
$expire_claim = $issuedat_claim + 3600;

$token = [
    "iss" => $issuer_claim,
    "aud" => $audience_claim,
    "iat" => $issuedat_claim,
    "nbf" => $issuedat_claim,
    "exp" => $expire_claim,
    "data" => [
        "userid" =>  (string)$user['usersId'] ?? "0",
        "username" =>  $user['userName'] ?? "unknown",
        "email" => $user['email'] ?? "unknown",
        "role" => $user['role'] ?? "user"
    ]
];

$jwt = JWT::encode($token, $secret_key, 'HS256');

echo json_encode([
    "success" => true,
    "token" => $jwt
]);

?>
