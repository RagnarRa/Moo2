'use strict'

const express = require('express');
const bodyParser = require('body-parser');
const users = require('./users'); //The . is for this directory, if only users, it'd search npm_modules

const app = express();
app.use(bodyParser.json());

const kafka = require('kafka-node');
const HighLevelProducer = kafka.HighLevelProducer;
const client = new kafka.Client('localhost:2181');
const producer = new HighLevelProducer(client); 

//Creates a new user in the DB
app.post('/api/user', (req, res) => {
	const data = req.body;
	if (!data.hasOwnProperty('username') || 
		!data.hasOwnProperty('age') ||
		!data.hasOwnProperty('email') || 
		!data.hasOwnProperty('password')) {
		res.status(412).send('User does not have all the required properties.');
		return; 
	}

	if ( (data.age !== parseInt(data.age, 10) ) || data.age < 0) {
		res.status(412).send('User age not valid.');
		return;
	}

	if (!(typeof data.username === "string"))  {
		res.status(412).send('Username must be a string.');
		return;
	}
	else if (data.username.length === 0) {
		res.status(412).send('Username must not be empty.');
		return;
	} 
	else if (data.email.length === 0) {
		res.status(412).send('Email must not be empty.');
		return;
	}

	//Verify that no user has this user's email or username
	users.getUsers({ $or: [ { 'username': data.username }, { 'email': data.email } ] }, function(err, docs) {
		
		if (docs.length > 0) {
			res.status(409).send('Username or email taken.');
			return;
		}

		users.addUser(data, (err, dbrs) => {
			if (err) {
				res.status(500).send('Unable to insert user.');
				return;
			}

			//Send the posted user to the users topic
			const kafkaUser = [{ 
			    topic: 'users',
			    messages: JSON.stringify(data)     //Stringify the user
			}]; 

			//Send the request to kafka.. 
			producer.send(kafkaUser, (err, data) => { 
			    if (err) {       
			    	console.log('Error:', err);  
		    	    return;     
		    	}   
		    }); 

			//return the last insertedId or the empty string
			res.status(201).send(dbrs.insertedIds[0] || '');
		});
	});
});

//Posts the user's username and password.. if incorrect.. 401.. if not.. we send back the user's token..
app.post('/api/token', (req, res) => {
		var data = req.body;
		if (!data.hasOwnProperty('username') || 
		!data.hasOwnProperty('password')) {
			res.status(412).send('Body must include username and password');
			return;
		}

		users.getUsers({ 'username' : data.username, 'password' : data.password }, function(err, docs) {
			if (docs.length > 0) {
				res.send(docs[0].token);
				return;
			}

			res.status(401).send('Unauthorized');
		}); 
});

//When the producer establishes a connection with the broker.. ready event is emitted.. 
producer.on('ready', () => { 
  console.log('Kafka producer is ready');
  app.listen(4000, () => { 
      console.log('Server starting on port 4000'); 
   }); 
}); 