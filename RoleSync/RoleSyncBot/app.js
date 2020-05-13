const Discord = require('discord.js');
const client = new Discord.Client();
const config = require('./config.json');
const net = require('net');
const fs = require('fs');

var database = "";

if (fs.existsSync('./data.json'))
    database = fs.readFileSync('./data.json')

var jsondatabase = JSON.parse(database);

function saveDataBase() {
    fs.writeFileSync('./data.json', JSON.stringify(jsondatabase));
}

var stdin = process.openStdin();

stdin.addListener('data', function (d) {

});

client.on('ready', () => {
    console.log(`Logged in as ${client.user.tag}!`);
});

client.on('message', msg => {
    if (msg.content.startsWith(config.prefix)) {
        var args = msg.content.split(' ');
        switch (args[0]) { // command
            case config.prefix + 'sync':
                if (args.length === 2 && !Number.isNaN(Number.parseInt(args[1])) && args[1].length === 16) {
                    jsondatabase.players[msg.author.id] = Number.parseInt(args[1]);
                    msg.reply('```diff\n+ Steam64Id Connected```');
                }
                else {
                    msg.reply('```diff\n- Missing Steam64Id```');
                }
                break;
            default:
                msg.reply('```diff\n- Invalid Command```');
                break;
        }
    }
});

client.login(config.token);

var server = net.createServer(function (sock) {
    console.log('OPENED: IP=' + sock.remoteAddress + ' PORT=' + sock.remotePort);
    sock.on('data', function (data) {
        try {
            var jdata = JSON.parse(data);
            if (jdata.command === "playerjoin") {
                var keys = Object.keys(jsondatabase.players);
                var objs = Object.values(jsondatabase.players);
                for (var i = 0; i < objs.length; i++) {
                    if (objs[i] === jdata.steamid) { //steamid check


                        var guild = client.guilds.resolve(config.guild);
                        var mem = guild.member(keys[i]);
                        for (var j = 0; j < config.roles.length; j++) { // get all roles
                            var role = mem.roles.cache.get(config.roles[j].id);
                            if (role !== undefined) { // has role check

                                var sendme = {
                                    'role': config.roles[j].ign,
                                    'id': jdata.steamid
                                };
                                sock.write(JSON.stringify(sendme) + ';');
                            }
                        }


                    }
                }
                /*if (jdata.steamid in jsondatabase.players) {
                    jsondatabase.players[jdata.steamid]
                }
                else {

                }*/
            }
        }
        catch (e) {
            console.log(e);
        }
    });

    sock.on('close', function (data) {
        console.log('CLOSED: IP=' + sock.remoteAddress + ' PORT=' + sock.remotePort);
    });
});

server.listen(config.port, function () {
    console.log('LISTEN on port ' + config.port);
});