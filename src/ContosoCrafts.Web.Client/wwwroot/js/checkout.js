
// We'll initialize these on page load and mount the
// Stripe Element for the card input. We need access to
// `stripe` and `cardElement` later when we checkout.
let stripe, cardElement;

async function registerElements(pubKey) {
  stripe = Stripe(pubKey);
  const elements = stripe.elements();
  cardElement = elements.create("card", {
    style: {
      base: {
        color: '#495057',
        fontWeight: 400,
      },
    },
  });

  cardElement.mount("#card-element");
}

async function checkout(paymentIntentClientSecret) {
  console.log(`Checkout called with: ${paymentIntentClientSecret}`);

  const cardholderNameInput = document.querySelector("#cardholder-name");
  const cardError = document.querySelector("#card-error");
  cardError.setAttribute("hidden", true);

  const {paymentIntent, error} = await stripe.confirmCardPayment(
    paymentIntentClientSecret, {
      payment_method: {
        card: cardElement,
        billing_details: {
          name: cardholderNameInput.value,
        }
      },
      // return_url: `${window.location.origin}/checkout/success`
    }
  )


  if(error) {
    cardError.innerText = error.message;
    cardError.removeAttribute("hidden");
  } else {
    DotNet.invokeMethod("ContosoCrafts.Web.Client", "ClearCart");
    $('#checkoutModal').modal('hide');
    cardElement.clear();
    window.location.href = "/checkout/success";
  }
}
