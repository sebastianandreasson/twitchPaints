var async = require('async'),
    moment = require('moment'),
    irc = require("tmi.js"),
    _ = require('underscore'),
    color = require('color'),
    debug = require('debug')('chat_bot');

// process.on('uncaughtException', function (err) {
//     console.error(err);
//     console.log("Node NOT Exiting...");
// });

var chatBot = {};
chatBot.players = {};
chatBot.votes = {};
charRegex = new RegExp("^[a-zA-Z ]+$");

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
        switch (chatBot.state) {
            case "painting":
                chatBot.handleChat(message, user);
                break;
            case "voting":
                chatBot.handleVoting(message, user);
                break;
            default:
                break;
        };
    });

    chatBot.client.addListener('part', function(channel, username){
        debug('parted:' + username);
    });

    chatBot.client.addListener('error', function(err){
        debug(err);
    });

    chatBot.client.connect();
};

chatBot.handleChat = function(message, user){
    console.log(players);
    if (message.indexOf("paint") === 0) {
        var args = message.slice(6, message.length).split(" ");
        var alpha = 1;
        var size = 1;
        args = _.map(args, function(s, i){
            if (i === 0 || i === 1){
                return parseInt(s);
            } else if (i === 2 && s.indexOf(",") != -1){
                return "rgb(" + s + ")";
            }
            return s;
        });
        var rgb;
        console.log("parse color");
        try {
            rgb = color(args[2]).rgb();
        } catch (e) {
            e = new Error(e);
            throw e;
        }
        var req = {
            name: "paint",
            payload: {
                x: args[0],
                y: args[1],
                rgb: rgb,
                size: args.length > 4 ? parseInt(args[4]) : 1,
            }
        };
        req.payload.rgb["a"] = args.length > 3 ? parseFloat(args[3]) : alpha;
        console.log(req);
        chatBot.players[user] = req.payload;
        if (chatBot.send) chatBot.send(req);
    }
    // else if (message === "save") {
    //     if (chatBot.send) chatBot.send({ name: "savePainting", payload: {}});
    // }
    // else if (message === "clear") {
    //     if (chatBot.send) chatBot.send({ name: "newPainting", payload: { width: 100, height: 100 }});
    // }
    else if (chatBot.players[user]){
        switch (message) {
            case "left":
                 chatBot.players[user].x -= chatBot.players[user].size;
                break;
            case "right":
                 chatBot.players[user].x += chatBot.players[user].size;
                break;
            case "up":
                 chatBot.players[user].y -= chatBot.players[user].size;
                break;
            case "down":
                 chatBot.players[user].y += chatBot.players[user].size;
                break;
            default:
                break;
        }
        if (chatBot.send) chatBot.send({ name: "paint", payload: chatBot.players[user] });
    }
};

chatBot.handleVoting = function(message, user){
    if (chatBot.votes[message]){
        chatBot.votes[message]++;
    }
    else{
        chatBot.votes[message] = 1;
    }
};

chatBot.getVotingList = function(){
    var list = _.chain(chatBot.votes)
    .map(function(val, key){
        return { name: key, votes: val };
    })
    .sortBy(function(o){
        return -o.votes;
    })
    .value().slice(0, 10);
    return list;
}
chatBot.getVotingWinner = function(){
    if (chatBot.votes){
        var list = _.chain(chatBot.votes)
        .map(function(val, key){
            return { name: key, votes: val };
        })
        .sortBy(function(o){
            return -o.votes;
        })
        .value().slice(0, 1);
        return list[0];
    }
    else {
        return { name: "aName" };
    }
};

chatBot.setState = function(state){
    debug("SET CHATSTATE: " + state);
    chatBot.state = state;
    switch (state) {
        case "painting":
            chatBot.players = {};
            break;
        case "voting":
            chatBot.votes = {};
        default:
            break;
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

// if (args.length > 5){
//     var rgbcolor = args.slice(2).join(",");
//     var brush = 1;
//     if (args.length >= 6){
//         brush = args.slice(args.length-1, args.length);
//     }
//     args = args.slice(0,2);
//
//     args.push("rgb(" + rgbcolor + ")");
//     args.push(brush);
// }
