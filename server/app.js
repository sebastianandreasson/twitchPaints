var io = require('socket.io').listen(1338),
    async = require('async'),
    moment = require('moment'),
    _ = require('underscore'),
    express = require('express'),
    bodyParser = require('body-parser'),
    debug_socket = require('debug')('socket');
    debug = require('debug')('server');

var app = express();
io.set('log level', 1);

var requestQueue = async.queue(function(request, callback){
    debug(request);
    if (connectedClient){
        connectedClient.emit(request.name, request.payload);
    }
    callback();
});

var chatBot = require('./chatbot.js');
chatBot.addListener(requestQueue);
chatBot.newStream("sebastaindeveloperaccount");

io.sockets.on('connection', function (socket) {
    connectedClient = socket;
    debug_socket('socket is connected!');
    setTimeout(function(){
        socket.emit("newPainting", { width: 100, height: 100 });
    }, 1000);

    socket.on("newPainting", function(){
        socket.emit("newPainting", { width: 100, height: 100 });
    });

    socket.on('error', function(e){
       debug(e);
    });

    socket.on('disconnect', function(){
        if (socket === connectedClient){
            connectedClient = null;
        }
    });

});

app.use(bodyParser());
app.listen(3000);
