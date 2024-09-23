import { useState, useEffect } from 'react';
import Solflare from '@solflare-wallet/sdk';
import { Connection } from '@solana/web3.js';

export const useConnection = () => {
  const [error, setError] = useState<string | null>(null);  // Храним ошибку, если она произойдет
  const [connection, setConnection] = useState<Connection | null>(null);


  const initializeConnection = () => {
    const conn = new Connection('https://api.devnet.solana.com');
    setConnection(conn);
  };

  useEffect(() => {
    try {
      initializeConnection();
    } catch(err: any)
    {
      setError(err.message || 'Failed to connect to wallet');
    }
  }, []);
  

  return { connection, error };
};

export const useWallet = () => {
  const [error, setError] = useState<string | null>(null);  // Храним ошибку, если она произойдет
  const [wallet, setWallet] = useState<Solflare | null>(null);

  const initializeWallet = async () => {
    const solflareWallet = new Solflare({network: 'devnet'}); // Создаем экземпляр Solflare
    await solflareWallet.connect(); // Выполняем подключение
    setWallet(solflareWallet);
  };

  useEffect(() => {
    try {
      initializeWallet();
    } catch(err: any)
    {
      setError(err.message || 'Failed to connect to wallet');
    }
  }, []);


  return { wallet, error };
};