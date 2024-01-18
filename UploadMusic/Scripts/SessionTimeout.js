function IdleTimeOut() {
    $(document).userTimeout({

        // ULR to redirect to, to log user out
        logouturl: '/Login/Index',

        // URL Referer - false, auto or a passed URL     
        referer: 'auto',

        // Name of the passed referal in the URL
        refererName: '/Admin/AdminPage',

        // Toggle for notification of session ending
        notify: true,

        // Toggle for enabling the countdown timer
        timer: true,

        // 10 Minutes in Milliseconds, then notification of logout
        session: 3000,

        // 5 Minutes in Milliseconds, then logout
        force: 9000,

        // Model Dialog selector (auto, bootstrap, jqueryui)              
        ui: 'auto',

        // Shows alerts
        debug: false,

        // <a href="https://www.jqueryscript.net/tags.php?/Modal/">Modal</a> Title
        modalTitle: 'Session Timeout',

        // Modal Body
        modalBody: 'You\'re being timed out due to inactivity. Please choose to stay signed in or to logoff. Otherwise, you will logged off automatically.',

        // Modal log off button text
        modalLogOffBtn: 'Log Off',

        // Modal stay logged in button text        
        modalStayLoggedBtn: 'Stay Logged In'

    });
}

