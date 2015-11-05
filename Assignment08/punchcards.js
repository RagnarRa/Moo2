'use strict'

const mongodb = require('mongodb');
const MongoClient = mongodb.MongoClient;
const mongoUrl = 'mongodb://localhost:27017/app'; //Connection string
const users = require('./users'); //The . is for this directory, if only users, it'd search npm_modules
const companies = require('./companies');

//Will be used to throw an exception containing the HTTP status code and error message
function HTTPException(statusCode, message) {
	this.statusCode = statusCode;
	this.message = message; 
}


//We'll allow the user to pass in a query (or empty query if no condition)
const getPunchcards = function(query, cb) {
	MongoClient.connect(mongoUrl, (err, db) => {
		if (err) {
			cb(err);
			db.close();
			return;
		}

		const collection = db.collection('punchcards');
		collection.find(query).toArray(function(err, docs) {
			cb(null, docs);
			db.close();
		});
	});
};


//Köllum í cb þegar við erum búnir að add-a þessu inn.. 
//cb skilar einhverjum status code
//token er userId
const addPunchcard = function(punchcard, cb) {
	//Check that we can get a user with this token, the token is the user_id
	users.getUsers({ "token" : punchcard.user_id }, (err, docs) => {
		if (!(docs.length > 0)) {
			cb(null, null, new HTTPException(401, 'Invalid token'));
			return;
		}

		var punchcard_lifetime = 0;
		//Check that we can find a company with this Id
		companies.getCompanies({ "_id" : punchcard.company_id }, (err, docs) => {
			if (!(docs.length > 0)) {
				cb(null, null, new HTTPException(404, 'Company not found'));
				return;
			} else {
				punchcard_lifetime = docs[0].punchcard_lifetime;
			}

			//Check that the user doesn't already have an active punchcard with this company
			getPunchcards({"company_id" : punchcard.company_id, "user_id" : punchcard.user_id}, (err, docs) => {
				for (var i = 0; i < docs.length; i++) {
					var created = docs[i].created;

					//Should probably use something better. Like MomentJS. Basically attempting to calculate the difference in days between the last punchcard the user has with the company
					var dateNow = new Date();
					var timeDiff = Math.abs(dateNow.getTime() - created.getTime());
					var diffDays = Math.floor(timeDiff / (1000 * 3600 * 24)); 
					if (diffDays < punchcard_lifetime) { //This punchcard is still active.. 
						cb(null, null, new HTTPException(409, 'The user already has an active punchcard with this company.'));
						return;
					}
				}

				//Insert the punchcard.. 
				MongoClient.connect(mongoUrl, (err, db) => {
					if (err) { //if we cannot connect
						cb(err);
						db.close();
						return; 
					}

					const collection = db.collection('punchcards');
					collection.insert(punchcard, function(ierr, res) {
						if (ierr) {
							cb(ierr);
							db.close();
							return;
						}

						//If no error, we call our callback with an array of created users.. will have one element.. array called "ops", then we have insertedCount and insertedIds in an array.
						cb(null, res); 
						db.close();
					});
				});
			});
		});
	});
};

//Export the function outside of this js file, others can then require this.
module.exports = {
	addPunchcard: addPunchcard,
	getPunchcards: getPunchcards,
};