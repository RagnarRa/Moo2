'use strict'

const express = require('express');
const bodyParser = require('body-parser');
const ObjectID = require('mongodb').ObjectID;
const users = require('./users'); //The . is for this directory, if only users, it'd search npm_modules
const companies = require('./companies');
const punchcards = require('./punchcards');
const uuid = require('uuid');
const ADMIN_TOKEN = '2632dcfd-ed04-4438-885f-bdf9450c5af8';

const app = express();
app.use(bodyParser.json());

//Get a list of all users
app.get('/api/user', (req, res) => {
	users.getUsers({}, function(err, docs) {
		//Send in the {} query to get all users
		res.send(docs);
	}); 
});

//Gets a user by his _id field. 
app.get('/api/user/:id', (req, res) => {
	const id = req.params.id;
	if (!ObjectID.isValid(id)) {
		res.status(412).send('Invalid user id. Is not a valid ObjectID.');
		return;
	}

	users.getUsers({'_id': new ObjectID(id) }, (err, docs) => {
		if (docs.length > 0) {
			res.send(docs[0]); //If we got one.. we can also use findOne which returns a single user, but find returns a list..
		} else {
			res.status(404).send('No user found with that id!');
		}
	});
});

//Creates a new user in the DB
app.post('/api/user', (req, res) => {
	const data = req.body;
	if (!data.hasOwnProperty('name') || 
		!data.hasOwnProperty('age') ||
		!data.hasOwnProperty('gender')) {
		res.status(412).send('User does not have all the required properties.');
		return; 
	}

	if (data.gender !== 'm' && data.gender !== 'f' && data.gender !== 'o') {
		res.status(412).send('User gender incorrect. Must be m, f, or o.');
		return;
	}

	if ( (data.age !== parseInt(data.age, 10) ) || data.age < 0) {
		res.status(412).send('User age not valid.');
		return;
	}

	if (!(typeof data.name === "string"))  {
		res.status(412).send('User name must be a string.');
		return;
	}
	else if (data.name.length === 0) {
		res.status(412).send('User name must not be empty.');
		return;
	}

	//Add token
	data.token = uuid.v4();
	//Error and response..
	users.addUser(data, (err, dbrs) => {
		if (err) {
			res.status(500).send('Unable to insert user.');
			return;
		}

		//return the last insertedId or the empty string
		res.status(201).send(dbrs.insertedIds[0] || '');
	});
});

//Gets a list of all companies
app.get('/api/company', (req, res) => {
	companies.getCompanies({}, function(err, docs) {
		//Send in the {} query to get all users
		res.send(docs);
	}); 
});

app.get('/api/company/:id', (req, res) => {
	const id = req.params.id;
	if (!ObjectID.isValid(id)) {
		res.status(412).send('Invalid company id. Is not a valid ObjectID.');
		return;
	}

	companies.getCompanies({'_id': new ObjectID(id) }, (err, docs) => {
		if (docs.length > 0) {
			res.send(docs[0]); //If we got one.. we can also use findOne which returns a single company, but find returns a list..
		} else {
			res.status(404).send('No company found with that id!');
		}
	});
});

//POSTs a company, needs admin authentication.. 
app.post('/api/company', (req, res) => {
	const headers = req.headers;

	console.dir(req.headers);

	if (!headers.hasOwnProperty('admin_token') || headers.admin_token !== ADMIN_TOKEN) {
		res.status(401).send('Invalid authentication.');
		return; 
	}

	const data = req.body;
	if (!data.hasOwnProperty('name') || 
		!data.hasOwnProperty('description') ||
		!data.hasOwnProperty('punchcard_lifetime')) {
		res.status(412).send('Company does not have all the required properties.');
		return; 
	}

	if ( (data.punchcard_lifetime !== parseInt(data.punchcard_lifetime, 10) ) || data.punchcard_lifetime < 0) {
		res.status(412).send('Company\'s punchcard_lifetime not valid.');
		return;
	}

	if (!(typeof data.name === "string"))  {
		res.status(412).send('Company name must be a string.');
		return;
	}
	else if (data.name.length === 0) {
		res.status(412).send('Company name must not be empty.');
		return;
	}

	if (!(typeof data.description === "string"))  {
		res.status(412).send('Company description must be a string.');
		return;
	}
	else if (data.description.length === 0) {
		res.status(412).send('Company description must not be empty.');
		return;
	}

	//Error and response..
	companies.addCompany(data, (err, dbrs) => {
		if (err) {
			res.status(500).send('Unable to insert company.');
			return;
		}

		//return the last insertedId or the empty string
		res.status(201).json({ "company_id" : dbrs.insertedIds[0] || ''});
	});
});

//Creates a punchcard
app.post('/api/punchcard/:company_id', (req, res) => {
	const headers = req.headers;

	//Check that the TOKEN is included
	if (!headers.hasOwnProperty('token')) {
		res.status(401).send('Invalid authentication.');
		return; 
	}

	const companyId = req.params.company_id;
	const data = req.body;
	data.created = new Date();
	data.user_id = req.headers.token;

	//Check that the companyId is a valid OjectID
	if (!ObjectID.isValid(companyId)) {
		res.status(412).send('CompanyId is invalid. Not an objectID.');
		return;
	} 

	data.company_id = new ObjectID(companyId);

	//Finally.. we can add one.. we send in a callback that takes an error (DB related) and an ex for a user defined exception.. with the given statusCode/message
	punchcards.addPunchcard(data, (err, dbrs, ex) => {
		if (err) {
			res.status(500).send('Unable to insert punchcard.');
			return;
		}

		if (ex) {
			res.status(ex.statusCode).send(ex.message);
			return;
		}

		//Return the last insertedId or the empty string
		res.status(201).send(dbrs.insertedIds[0] || '');
	}); 
});


app.listen(4000);