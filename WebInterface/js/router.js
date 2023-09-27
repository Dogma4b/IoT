let router = new Navigo("/");
let Modules = {};

fetch("/modules/register.json").then(response => response.json()).then(data => {
    for(let category in data) {
        let categoryDOM = sidebar.CreateCategory(data[category].name, data[category].icon);
        categoryDOM.Enable();
        for(let item in data[category].items) {
            let href = `/` + category + `/` + item;
            router.on(href, () => {
                new module(category + "." + item).render();
                new Function(data[category].items[item].logic)();
            });
            categoryDOM.AddItem(data[category].items[item].name, data[category].items[item].icon, href);
        }
    }

    router.resolve();
});