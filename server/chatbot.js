var async = require('async'),
    moment = require('moment'),
    irc = require("tmi.js"),
    _ = require('underscore'),
    color = require('color'),
    debug = require('debug')('chat_bot');

var chatBot = {},
    charRegex = new RegExp("^[a-zA-Z ]+$"),
    players = [];

chatBot.connect = function(channel){
    if (chatBot.client){
        chatBot.client = null;
    }
    chatBot.client = new irc.client({
        options: {
            debug: false
        },
        connection: {
            random: "chat",
            reconnect: true
        },
        identity: {
            username: "octonombot",
            password: "oauth:qwfselxf5nxlb1eimaeys4u4axuc35"
        },
        channels: ["#" + channel]
    });

    chatBot.client.on('error', function(){
    });

    chatBot.client.addListener('chat', function (channel, user, message) {
        debug(user.username + ': ' + message);
        handleChat(message);
    });

    chatBot.client.addListener('part', function(channel, username){
        debugVerbose('parted:' + username);
    });

    chatBot.client.addListener('error', function(err){
        debug(err);
    });

    chatBot.client.connect();
};

var handleChat = function(message){
    if (message.indexOf("paint") === 0){
        var args = message.slice(5, message.length).split(",");
        console.log(args);
        if (args.length === 3){

        }
        args = _.map(args, function(s, i){
            if (i === 0 || i === 1){
                return parseInt(s);
            } else {
                return s;
            }
        });
        console.log(args);
        var req = {
            name: "paint",
            payload: {
                x: args[0],
                y: args[1],
                rgb: color(args[2]).rgbArray()
            }
        };
        if (chatBot.send) chatBot.send(req);
    }
};

chatBot.mode = "idle";
chatBot.addListener = function(requestQueue){
    chatBot.send = requestQueue.push;
};
chatBot.newStream = function(channel){
    chatBot.connect(channel);
};
chatBot.endStream = function(){
    if (chatBot.client){
        chatBot.client.disconnect();
        chatBot.client = null;
    }
};

module.exports = chatBot;
