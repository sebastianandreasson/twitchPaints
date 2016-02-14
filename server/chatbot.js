import async from "async";
import moment from "moment";
import irc from "tmi.js";
import _ from "underscore";
import color from "color";
import debug_module from "debug";

const debug = debug_module("chat_bot");

process.on('uncaughtException', function (err) {
    console.error(err);
    console.log("Node NOT Exiting...");
});

var chatBot = {};
chatBot.players = {};
chatBot.votes = {};

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

    chatBot.client.on("error", () =>{
    });

    chatBot.client.addListener("chat", (channel, user, message) => {
        debug(user.username + ": " + message);
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

    chatBot.client.addListener("part", (channel, username) => {
        debug("parted: " + username);
    });

    chatBot.client.addListener("error", (err) => {
        debug(err);
    });

    chatBot.client.connect();
};

chatBot.handleChat = (message, user) => {
    console.log(chatBot.players);
    if (message.indexOf("paint") === 0) {
        var args = message.slice(6, message.length).split(" ");
        var alpha = 1;
        var size = 1;
        args = _.map(args, (s, i) => {
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
        chatBot.players[user] = req.payload;
        if (chatBot.send) chatBot.send(req);
    }
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

chatBot.handleVoting = (message, user) => {
    if (chatBot.votes[message]){
        chatBot.votes[message]++;
    }
    else{
        chatBot.votes[message] = 1;
    }
};

chatBot.getVotingList = () => {
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
chatBot.getVotingWinner = () => {
    if (chatBot.votes && Object.keys(chatBot.votes).length > 0){
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

chatBot.setState = (state) => {
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
chatBot.addListener = (requestQueue) => {
    chatBot.send = requestQueue.push;
};
chatBot.newStream = (channel) => chatBot.connect(channel);

chatBot.endStream = () => {
    if (chatBot.client){
        chatBot.client.disconnect();
        chatBot.client = null;
    }
};

module.exports = chatBot;
