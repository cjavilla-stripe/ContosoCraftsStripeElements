// We'll initialize these on page load and mount the
// Stripe Element for the card input. We need access to
// `stripe` and `cardElement` later when we checkout.
let stripe, cardElement;

async function registerElements(pubKey) {
  stripe = Stripe(pubKey);
  const elements = stripe.elements();
  cardElement = elements.create("card");
  cardElement.mount("#card-element");
}

async function checkout(paymentIntentClientSecret) {
  console.log(`Checkout called with: ${paymentIntentClientSecret}`);
  const {paymentIntent, error} = await stripe.confirmCardPayment(
    paymentIntentClientSecret, {
      payment_method: {
        card: cardElement,
      }
    }
  )
  if(error) {
    console.log(`Failed: ${error.message}`);
  } else {
    alert(`Payment Succeeded!`);
  }

  console.log(`Checking out with payment intent: ${paymentIntentClientSecret}`);
}
