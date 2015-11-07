'use strict';  

const kafka = require('kafka-node'); 
const HighLevelConsumer = kafka.HighLevelConsumer; 
const client = new kafka.Client('localhost:2181'); //Kafka broker 
const uuid = require('uuid');
const users = require('./users'); //The . is for this directory, if only users, it'd search npm_modules

//Consume listens for messages on the requests topic
const consumer = new HighLevelConsumer(client, [ { topic: 'users' }, ], 
												 {  // The consumer group that this consumer is part of. 
												 	groupId: 'mygroup'         
												 } ); 
consumer.on('message', function(message) {
   //message includes topic, partition, etc. and value is the object itself
   const value = JSON.parse(message.value);  //The object itself.. the user.. 
   var token = uuid.v4();
   users.addTokenToUser(value.username, token);
}); 