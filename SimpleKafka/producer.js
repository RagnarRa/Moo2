'use strict';  

const express = require('express'), 
  	  kafka = require('kafka-node'),
  	  app = express(), 
  	  port = 4000; 
const HighLevelProducer = kafka.HighLevelProducer;
const client = new kafka.Client('localhost:2181');
const producer = new HighLevelProducer(client); 

//Middleware.. for each incoming request.. 
app.use((req, res, next) => {   
	const request_details = {  
		'path': req.path, 
		'headers': req.headers,  
		'method': req.method,
	};    

	const data = [{ 
	    topic: 'requests',
	    messages: JSON.stringify(request_details),     
	}]; 

	//Send the request to kafka.. 
	producer.send(data, (err, data) => { 
	    if (err) {       
	    	console.log('Error:', err);  
    	    return;     
    	}   

    	console.log(data);  
	    next();  //Jump to next middleware
    }); 
});  

app.get('/', (req, res) => { 
  res.send('Hello world'); 
}); 

//When the producer establishes a connection with the broker.. ready event is emitted.. 
producer.on('ready', () => { 
  console.log('Kafka producer is ready');
  app.listen(port, () => { 
      console.log('Server starting on port', port); 
   }); 
}); 