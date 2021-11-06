import { Elements } from "@stripe/react-stripe-js";
import { loadStripe } from "@stripe/stripe-js";
import { useState, useEffect } from 'react';
import { useAppDispatch } from "../../app/store/configureStore";
import CheckoutPage from "./CheckoutPage";
import agent from '../../app/api/agent';
import { setBasket } from "../basket/basketSlice";
import LoadingComponent from "../../app/layout/LoadingComponent";

const stripePromise = loadStripe('pk_test_51IgB4nA0YWUby5DnuuWQ2rEpICQkLfmG0lDVxK2aS6KRZkNEyo22C56zIh2GR1tonNMdRuZ0cjbKiLKeRR0DyAZ600mZiX91a6');

export default function CheckoutWrapper() {
    const dispatch = useAppDispatch();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        agent.Payments.createPaymentIntent()
            .then(basket => dispatch(setBasket(basket)))
            .catch(error => console.log(error))
            .finally(() => setLoading(false));
    }, [dispatch]);

    if (loading) return <LoadingComponent message='Loading checkout...' />

    return (
        <Elements stripe={stripePromise}>
            <CheckoutPage />
        </Elements>
    )
}
