import { Button, CircularProgress, createTheme, Paper, ThemeProvider, Typography } from '@mui/material';
import './App.css';
import { useAuth } from 'react-oidc-context';

const draculaTheme = createTheme({
  palette: {
    mode: 'dark', // Темная тема
    primary: {
      main: '#ee8611', // Оранжевый
    },
    secondary: {
      main: '#C70039', // Розовый
    },
    background: {
      default: '#272626', // Темный фон
      paper: '#272626', // Фон для карточек
    },
    text: {
      primary: '#f8f8f2', // Белый текст
      secondary: '#6272a4', // Серый текст
    },
  },
});

const App = () => {
  const auth = useAuth();

  switch (auth.activeNavigator) {
    case 'signinSilent':
      return (
        <Paper elevation={3} style={{ padding: '20px', textAlign: 'center' }}>
          <Typography variant="h5">Signing you in...</Typography>
          <CircularProgress />
        </Paper>
      );
    case 'signoutRedirect':
      return (
        <Paper elevation={3} style={{ padding: '20px', textAlign: 'center' }}>
          <Typography variant="h5">Signing you out...</Typography>
        </Paper>
      );
    default:
      break;
  }

  if (auth.isLoading) {
    return (
      <Paper elevation={3} style={{ padding: '20px', textAlign: 'center' }}>
        <CircularProgress />
        <Typography variant="h5">Loading...</Typography>
      </Paper>
    );
  }

  if (auth.error) {
    return (
      <Paper elevation={3} style={{ padding: '20px', textAlign: 'center' }}>
        <Typography variant="h5">Oops... {auth.error.message}</Typography>
      </Paper>
    );
  }

  if (auth.isAuthenticated) {
    return (
      <Paper elevation={3} style={{ padding: '20px', textAlign: 'center' }}>
        <Typography variant="h5">Hello {auth.user?.profile.sub}</Typography>
        <Button
          variant="contained"
          color="primary"
          onClick={() => void auth.removeUser()}
        >
          Log out
        </Button>
      </Paper>
    );
  }

  return (
    <Paper elevation={3} style={{ padding: '20px', textAlign: 'center' }}>
      <Typography variant="h5">Please log in</Typography>
      <Button
        variant="contained"
        color="primary"
        onClick={() => void auth.signinRedirect()}
      >
        Log in
      </Button>
    </Paper>
  );
};

const AppWithTheme = () => {
  return (
    <ThemeProvider theme={draculaTheme}>
      <App />
    </ThemeProvider>
  );
};

export default AppWithTheme;