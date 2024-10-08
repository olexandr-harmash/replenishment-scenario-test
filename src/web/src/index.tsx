import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { AuthProvider } from "react-oidc-context";
import App from './App';
import reportWebVitals from './reportWebVitals';
import { User, WebStorageStateStore } from 'oidc-client-ts';

const oidcConfig = {
  authority: "https://localhost:5001/",
  client_id: "web",
  client_secret: "secret",
  redirect_uri: "http://localhost:3000/",
  post_logout_redirect_uri: "http://localhost:3000/",
  response_type: "code",  // или "id_token token"
  scope: "openid profile api1",
  // post_logout_redirect_uri: "https://your-app.com/logout",
  // silent_redirect_uri: "https://your-app.com/silent-renew",
  // automaticSilentRenew: true,
  //loadUserInfo: true,
  onSigninCallback: (user: User | void): void => {
    window.history.replaceState(
        {},
        document.title,
        window.location.pathname
    );
  },
  userStore: new WebStorageStateStore({ store: window.localStorage }),
};

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <AuthProvider {...oidcConfig}>
    <App />
  </AuthProvider>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
