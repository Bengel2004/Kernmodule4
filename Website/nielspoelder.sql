-- phpMyAdmin SQL Dump
-- version 4.9.4
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Gegenereerd op: 18 jun 2020 om 13:45
-- Serverversie: 10.2.31-MariaDB
-- PHP-versie: 5.5.14

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `nielspoelder`
--

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `scores`
--

CREATE TABLE `scores` (
  `id` int(11) NOT NULL,
  `game_id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `place` int(11) NOT NULL,
  `score` int(11) NOT NULL,
  `date_time` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Gegevens worden geëxporteerd voor tabel `scores`
--

INSERT INTO `scores` (`id`, `game_id`, `user_id`, `place`, `score`, `date_time`) VALUES
(1, 1, 3, 1, 59, '2020-05-14'),
(2, 3, 3, 2, 15, '2020-05-02'),
(3, 5, 4, 2, 73, '2020-05-05'),
(4, 5, 2, 1, 14, '2020-05-12'),
(5, 5, 1, 1, 24, '2020-05-12'),
(6, 3, 5, 2, 63, '2020-05-12'),
(7, 1, 1, 1, 1, '2020-05-18'),
(8, 1, 1, 1, 1, '2020-05-18'),
(9, 1, 5, 3, 1, '2020-05-18'),
(10, 1, 1, 2, 1, '2020-06-08'),
(11, 1, 5, 2, 255, '2020-06-08'),
(12, 1, 5, 1, 255, '2020-06-08'),
(13, 1, 5, 2, 255, '2020-06-08'),
(14, 1, 1, 1, 900, '2020-04-15'),
(15, 1, 1, 1, 1, '2020-06-18'),
(16, 1, 1, 1, 4, '2020-06-18'),
(17, 1, 0, 1, 50, '2020-06-18'),
(18, 1, 0, 1, 350, '2020-06-18');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `servers`
--

CREATE TABLE `servers` (
  `id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL,
  `password` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

--
-- Gegevens worden geëxporteerd voor tabel `servers`
--

INSERT INTO `servers` (`id`, `name`, `password`) VALUES
(1, 'Server', 'Kipje');

-- --------------------------------------------------------

--
-- Tabelstructuur voor tabel `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `first_name` varchar(25) COLLATE utf8_unicode_ci NOT NULL,
  `last_name` varchar(35) COLLATE utf8_unicode_ci NOT NULL,
  `email` varchar(50) COLLATE utf8_unicode_ci NOT NULL,
  `password` varchar(100) COLLATE utf8_unicode_ci NOT NULL,
  `birthday` date NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

--
-- Gegevens worden geëxporteerd voor tabel `users`
--

INSERT INTO `users` (`id`, `first_name`, `last_name`, `email`, `password`, `birthday`) VALUES
(1, 'Kas', 'Kaas', 'kat@kaas.nl', 'Kipje', '1997-12-28'),
(2, 'Gerard', 'Klap Koek', 'gerard@gmail.com', 'Renvoorjeleven', '1952-12-12'),
(3, 'Kevin', 'Nearmos', 'gaas@kaas.nl', 'Kipje', '1997-12-28'),
(4, 'Joop', 'klapband Koek', 'joop.klapband@gmail.om', 'Gaatvoort', '1952-12-12'),
(5, 'Ben', 'Dover', 'GIBenDover@gmail.com', 'GaTochNaarDeGamma', '1996-12-13'),
(6, 'w', 'a', 's', 'd', '2020-05-11'),
(7, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(8, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(9, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(10, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(11, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(12, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(13, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(14, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(15, 'Jan', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(16, 'geeft', 'Jaap', 'sasd@ask.com', 'dasa', '2020-05-11'),
(18, 'pietje', 'rietje', 'gibendover@gefiltefish.com', 'Donderop', '2020-05-11');

--
-- Indexen voor geëxporteerde tabellen
--

--
-- Indexen voor tabel `scores`
--
ALTER TABLE `scores`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `servers`
--
ALTER TABLE `servers`
  ADD PRIMARY KEY (`id`);

--
-- Indexen voor tabel `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT voor geëxporteerde tabellen
--

--
-- AUTO_INCREMENT voor een tabel `scores`
--
ALTER TABLE `scores`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT voor een tabel `servers`
--
ALTER TABLE `servers`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT voor een tabel `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
