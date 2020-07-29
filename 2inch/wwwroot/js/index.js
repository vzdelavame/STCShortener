var app = new Vue({
    el: '#app',
    data: {
        activeBurger: false
    },
    methods: {
        showBurger() {
            this.activeBurger = !this.activeBurger;
        }
    }
  })