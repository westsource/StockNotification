-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               5.6.10 - MySQL Community Server (GPL)
-- Server OS:                    Win64
-- HeidiSQL version:             7.0.0.4053
-- Date/time:                    2013-04-20 20:53:45
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!40014 SET FOREIGN_KEY_CHECKS=0 */;

-- Dumping structure for table stocknotification.stock
-- COMMENT='股票代码';
CREATE TABLE IF NOT EXISTS `stock` (
  `id` varchar(50) NOT NULL PRIMARY KEY,
  `stock` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
); 

-- Data exporting was unselected.


-- Dumping structure for table stocknotification.user
-- COMMENT='用户';
CREATE TABLE IF NOT EXISTS `user` (
  `userid` varchar(50) NOT NULL PRIMARY KEY,
  `username` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  PRIMARY KEY (`userid`)
);

-- Data exporting was unselected.


-- Dumping structure for table stocknotification.userstock
-- COMMENT='用户关注股票'
CREATE TABLE IF NOT EXISTS `userstock` (
  `id` varchar(50) NOT NULL PRIMARY KEY,
  `userid` varchar(50) NOT NULL,
  `stockid` varchar(50) NOT NULL,
  PRIMARY KEY (`id`)
);

-- COMMENT='机构持股率'
CREATE TABLE IF NOT EXISTS 'stock_inst_own' (
  `id` varchar(50) NOT NULL PRIMARY KEY,
  `stockid` varchar(50) NOT NULL,
  'instown' int NOT NULL,
  'time' varchar(40) NOT NULL
) 	

-- Data exporting was unselected.
/*!40014 SET FOREIGN_KEY_CHECKS=1 */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
