import React, { ComponentType } from "react";
import { RouteComponentProps, RouteProps } from "react-router-dom";
import { useAppSelector } from '../store/configureStore';
import { Redirect, Route } from 'react-router';

interface Props extends RouteProps {
    component: ComponentType<RouteComponentProps<any>> | ComponentType<any>
}

export default function PrivateRoute({ component: Component, ...rest }: Props) {
    const {user} = useAppSelector(state => state.account);
    
    return (
      <Route
        {...rest}
        render={props =>
          user ? (
            <Component {...props} />
          ) : (
            <Redirect
              to={{
                pathname: "/login",
                state: { from: props.location }
              }}
            />
          )
        }
      />
    );
  }