window.onload = function () {
    render();
};

function render() {
    window.recaptchaVerifier = new firebase.auth.RecaptchaVerifier('recaptcha-container', {
        'size': 'normal',
        'callback': (response) => {
            // reCAPTCHA solved, allow sending the OTP
            document.getElementById("send-otp-button").removeAttribute("disabled");
        },
        'expired-callback': () => {
            // Reset the reCAPTCHA when it expires
            grecaptcha.reset(window.recaptchaWidgetId);
        }
    });
    recaptchaVerifier.render();
}

function phoneAuth() {
  
  
    var a = document.getElementById("number").value;
   
    var number = a;

    const appVerifier = window.recaptchaVerifier;
   
    firebase.auth().signInWithPhoneNumber(number, appVerifier)
        .then((confirmationResult) => {
          
            window.confirmationResult = confirmationResult;
            console.log("Message has been sent");
            this.submit();
        })
        .catch((error) => {
            handlePhoneAuthError(error);
        });
    }

function handlePhoneAuthError(error) {
    if (error.code == 'auth/too-many-requests' && error.message != 'TOO_SHORT') {
        alert("Too many requests: Please try again later.");
    } else if (error.message == 'TOO_SHORT') {
        alert("Please make sure you enter a valid number, add the Country Code like: +92334xxxx..");
    } else {
       // alert("Error Occurred");
    }
    console.error("Error from sending OTP to phone:", error);
}
function handleOTPVerificationError(error) {
    console.error("Error verifying code:", error);
    alert("Code not verified!");
}

function verifyOTPMember() {
   // alert("VerifyCalled");
    var otp = "";
    for (var i = 1; i < 7; i++) {
        var inputElement = document.querySelector(".form-control" + i);
        otp += inputElement.value;
    }
   

    if (otp.length === 6) {
        confirmationResult.confirm(otp)
            .then((result) => {
                console.log("Code verified:", result);
                window.location.href = "/Profile/Index";
              
            })
            .catch((error) => {
                handleOTPVerificationError(error);
            });
    } else {
        alert("Please enter a 6-digit code.");
    }
}
function verifyOTPOfficial() {
    // alert("VerifyCalled");
    var otp = "";
    for (var i = 1; i < 7; i++) {
        var inputElement = document.querySelector(".form-control" + i);
        otp += inputElement.value;
    }


    if (otp.length === 6) {
        confirmationResult.confirm(otp)
            .then((result) => {
                console.log("Code verified:", result);
                window.location.href = "/ElectionOfficials/CandidateVoteInfo";

            })
            .catch((error) => {
                handleOTPVerificationError(error);
            });
    } else {
        alert("Please enter a 6-digit code.");
    }
}
function verifyOTPCommissioner() {
    // alert("VerifyCalled");
    var otp = "";
    for (var i = 1; i < 7; i++) {
        var inputElement = document.querySelector(".form-control" + i);
        otp += inputElement.value;
    }


    if (otp.length === 6) {
        confirmationResult.confirm(otp)
            .then((result) => {
                console.log("Code verified:", result);
                window.location.href = "/ElectionCommissioner/Dashboard";

            })
            .catch((error) => {
                handleOTPVerificationError(error);
            });
    } else {
        alert("Please enter a 6-digit code.");
    }
}