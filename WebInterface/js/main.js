globalThis.ws = new WebSocket("ws://127.0.0.1:27840");
let IsAuthed = false;
globalThis.activeDevice = null;

let pug = require("pug");
fengari.load(`
    ws = js.global.ws

    function ws:sndl(data)
        ws:snd(object(data))
    end

    function object(t)
        local o = js.new(js.global.Object)
        for k, v in pairs(t) do
            assert(type(k) == "string" or js.typeof(k) == "symbol", "JavaScript only has string and symbol keys")
            o[k] = v
        end
        return o
    end
`)();

ws.snd = function(data) {this.send(JSON.stringify(data))}
ws.onopen = () => {
    //ConsoleBeauty.addCode("Connected")
    ws.snd({"login":"andrey","password":CryptoJS.SHA512("dsXAiqMo1d9Li35fdDazpS" + Date.now().toString().substr(0,9)).toString()});
}
ws.onclose = () => ConsoleBeauty.addCode("disconnected")
ws.onmessage = (msgEV) => module.packetHandler(JSON.parse(msgEV.data)); //ReceiveHandler(JSON.parse(msgEV.data));

globalThis.module = class module {

    static cache = {};
    static packets = {};
    static current;
    
    static init(name) {
        if (typeof(this.cache[name]) == "undefined") {
            const newModule = new classModule(name);
            this.cache[name] = newModule;
            return newModule;
        } else {
            return this.cache[name];
        }
    }

    static packetHandler(data) {
        console.log(data);
        if (typeof(this.packets[data.id]) != "undefined") {
            let m = this.packets[data.id];
            m.func(m.module, data);
        }
    }

    constructor(name) {
        if (typeof(module.cache[name]) != "undefined") {
            module.current = module.cache[name];
            return module.cache[name];
        } else {
            module.cache[name] = this;
            module.current = this;
            this.name = name;
            this.dir = "/modules/";
            this.template = [];
            this.data = {};
            this.objects = {};
            this.loaded = {js:false,lua:false};
            this.loadANDparse(this.dir + ((name.slice(-4) == ".wit") ? (name.substr(0, name.length - 4).replace(".", "/") + ".wit") : (name.replace(".", "/") + ".wit")));
        }
    }

    async loadANDparse(url) {
        fetch(url)
        .then(response => response.text())
        .then(data => {
            let parsedTemplate = new DOMParser().parseFromString(data, "text/html");
            for (let item of parsedTemplate.querySelector("head").children) {
                const itemData = {name:item.localName};

                for (let tag of item.attributes) {
                    itemData[tag.name] = tag.value;
                }
                itemData.data = item.innerHTML;
                
                this.template.push(itemData);
            }
        });
    }

    async render(data, callback) {
        if (this.template.length == 0) { setTimeout(() => {this.render(data);}, 10); return; }

        this.data = Object.assign(this.data, data);

        for (let item of this.template) {
            switch (item.name) {
                case "template":
                    if (typeof(item.include) != "undefined") item.data = await (await fetch(this.dir + item.include + ".pug")).text();
                    let html;
                    switch (item.lang) {
                        case "pug":
                            html = pug.compile(item.data.trim())(this.data);
                            break;
                        case "html":
                            html = item.data;
                            break;
                    }
                    document.querySelector("#" + item.tag).innerHTML = html;
                    break;
                case "style":
                    break;
                case "script":
                    switch (item.type) {
                        case "packet":
                            if (item.lang == "lua") {
                                let chunkName = this.name + ".packet." + item.id;
                                module.packets[item.id] = {module:this, func:fengari.load(`local args = {...} local module = args[2] local data = args[3] ` + item.data, chunkName)};
                            } else {
                                module.packets[item.id] = {module:this, func:new Function("module", "data", item.data)};
                            }
                            break;
                        default:
                            if (!this.loaded.js || !this.loaded.lua || item.update) {
                                if (item.lang == "lua") {
                                    fengari.load(`local args = {...} local module = args[2] ` + item.data, this.name)(this);
                                    this.loaded.lua = true;
                                } else {
                                    new Function("module", item.data)(this);
                                    this.loaded.js = true;
                                }
                            }
                            break;
                    }
                break;
            }
        }

        if (typeof(callback) != "undefined") callback()
    }
}

globalThis.sidebar = class {
    static categories = {}
    static #sidebar = document.querySelector("#sidebar");

    static CreateCategory = function(category, icon) {
        let catid = category.substr(0,5).toLowerCase();
        this.categories[catid] = new this.category(category, icon);
        this.#sidebar.appendChild(this.categories[catid].category);
        return this.categories[catid];
    }
    
    static AddItem = function(catid, item, icon, href) {
        if (typeof(this.categories[catid]) != "undefined") {
            this.categories[catid][item] = new this.item(item, icon, href);
            this.categories[catid].nav.appendChild(this.categories[catid][item].item);
        } else {
            console.log("Category not found");
        }
    }
    
    static category = class {

        constructor(name = "default", icon = "fa-code") {

            this.name = name;

            this.category = document.createElement("li");
            this.category.className = "nav-item has-treeview";

            this.a = document.createElement("a");
            this.a.href = "#";
            this.a.className = "nav-link";

            this.i = document.createElement("i");
            this.i.className = "nav-icon fas " + icon;

            this.p = document.createElement("p");
            this.p.innerHTML = "&nbsp" + name;

            this.p.i = document.createElement("i");
            this.p.i.className = "right fas fa-angle-left";

            this.p.appendChild(this.p.i);

            this.a.appendChild(this.i);
            this.a.appendChild(this.p);

            this.nav = document.createElement("ul");
            this.nav.className = "nav nav-treeview";
            
            this.category.appendChild(this.a);
            this.category.appendChild(this.nav);
        }

        AddItem = (item, icon, href) => sidebar.AddItem(this.name.substr(0,5).toLowerCase(), item, icon, href);
        Enable = () => {
            this.category.className += " menu-open";
            this.nav.style = "display: block;"
            this.a.className += " active";
        }
        Disable = () => {
            this.category.className += " menu-open";
            this.nav.style = "display: block;"
            this.a.className += " active";
        }
    }

    static item = class {
        
        constructor(name = "default", icon = "fa-code", href = "#") {
            this.item = document.createElement("li");
            this.item.className = "nav-item";

            this.a = document.createElement("a");
            this.a.href = href;
            this.a.className = "nav-link";
            this.a.setAttribute("client-routing", null);

            this.i = document.createElement("i");
            this.i.className = "nav-icon fas " + icon;

            this.p = document.createElement("p");
            this.p.innerHTML = "&nbsp" + name;

            this.a.appendChild(this.i);
            this.a.appendChild(this.p);

            this.item.appendChild(this.a);
        }
    }
}