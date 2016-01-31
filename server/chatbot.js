var async = require('async'),
    moment = require('moment'),
    irc = require("tmi.js"),
    _ = require('underscore'),
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
        debugVerbose(user.username + ': ' + message);
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
    debug(message);
    var _words = message.split(" ");
    var emotes = _.chain(_words)
    .filter(function(word){
        return _.find(twitchEmotes, function(emote){ return emote === word; });
    })
    .map(function(emote){
        var _id = emoteMapper.emotes[emote].image_id;
        return {
            name: emote,
            id:_id,
            imageURL: emoteMapper.template.medium.replace("{image_id}", _id)
        }
    })
    .value();
    debug(emotes);
    chatBot.send({ type: "message", message: message, emoticons: emotes });
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
