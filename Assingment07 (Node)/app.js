var express = require('express'); //include the express module 
var app = express(); //returns an Express server instance

//bodyParser() will be deprecated in connect 3.0, so we use the two lines below. Used to parse the body of, for an example, a POST request.. we need this middleware. Parses the body of the request and sets the body property on the request object. 
app.use(express.json()); //Since they're deprecating bodyParser, we'll use these two
app.use(express.urlencoded());

app.listen(process.env.PORT || 4730); //process.env.PORT means if the port is passed in we will use it. 

var companies = [], users = [];
var lastInsertedCompanyId = 0, lastUserId = 0; 

//Create a route.. pass in the URL and the callback method for it. 
//Should return a list of all registered companies
app.get('/api/companies', function(req, res) { //The callback accepts a request and a response object.. 
	res.json(companies); //Returns the array of companies as a JSON object 
});

//Should post a new company. The required fields are name and punchCount. punchCount is the number of .. punches.. a user needs to get a discount.. 
app.post('/api/companies', function(req, res) {
	if (!req.body.hasOwnProperty('name') || 
		!req.body.hasOwnProperty('punchCount')) {
		res.statusCode = 412;
		return res.send('412 - Precondition failed: Post syntax incorrect.');
	}

	var newCompany = {
		id: lastInsertedCompanyId++,
		name: req.body.name,
		punchCount: req.body.punchCount
	};

	companies.push(newCompany);
	res.statusCode = 201; 
	res.json(newCompany); //push back the created object
});

//Gets a company by ID
app.get('/api/companies/:id', function(req, res) {
	var companyId = req.params.id; //Indexes the array.. 
	//Check whether the id is invalid
	if (companies.length <= companyId || companyId < 0) {
		res.statusCode = 404;
		return res.send('404 - Company not found.');
	}

	res.json(companies[companyId]);
});

//Gets the array of users
app.get('/api/users', function(req, res) {
	res.json(users);
});

//Adds a user.. the required properties are: name, email
app.post('/api/users', function(req, res) {
	if (!req.body.hasOwnProperty('name') ||
		!req.body.hasOwnProperty('email')) {
		res.statusCode = 412;
		return res.send('412 - Precondition failed: Post syntac incorrect');
	}

	var newUser = {
		id: lastUserId++,
		name: req.body.name,
		email: req.body.email,
		punches: []
	};

	users.push(newUser);
	res.statusCode = 201;
	res.json(newUser); //TODO: Is this 201 created
});

//Should get a list of all punches for the given user
//A punch should look like this: { companyId: x, created: date }.
//Should be able to filter by adding ?company={id} (TODO)
app.get('/api/users/:id/punches', function(req, res) {
	var userId = req.params.id; //Used for indexing the user array
	//Check whether the id is invalid
	if (users.length <= userId || userId < 0) {
		res.statusCode = 404;
		return res.send('404 - User not found.');
	}

	//var listToReturn = users[req.params.id].punches;
	var listToReturn = [];

	if (req.query.company !== undefined) {
		//He wants a filtered list.. 
		for (var i = 0; i < users[userId].punches.length; i++) {
			if (users[userId].punches[i].companyId === parseInt(req.query.company)) {
				listToReturn.push(users[userId].punches[i]);
			}
		}
	} else {
		listToReturn = users[userId].punches; 
	}

	res.json(listToReturn);
});

//Add a punch to a user account... 
app.post('/api/users/:id/punches', function(req, res) {
	var userId = req.params.id; //Used for indexing the user array
	//Validate the user id
	if (users.length <= userId || userId < 0) {
		res.statusCode = 404;
		return res.send('404 - User not found.');
	}

	//Verify that the companyId is there..
	if (!req.body.hasOwnProperty('companyId')) { //TODO: Verify that this ID name is okay. 
		res.statusCode = 412;
		return res.send('412 - company ID missing.');
	}

	if (companies.length <= req.body.companyId || req.body.companyId < 0) {
		res.statusCode = 404;
		return res.send('404 - company not found.');
	}

	res.statusCode = 201; 
	var newPunch = { companyId: req.body.companyId, created: new Date() };
	users[userId].punches.push(newPunch);
	res.json(true);
});

