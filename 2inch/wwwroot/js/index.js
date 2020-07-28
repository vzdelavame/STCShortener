var app = new Vue({
    el: '#app',
    data: {
        seen: false,
        activeBurger: false
    },
    methods: {
        showBurger() {
            this.activeBurger = !this.activeBurger;
        }
    }
  })