import './App.css';
import { useEffect, useState } from 'react';
import { useConnection, useWallet } from './utils/providerConnect';
import createTransferTransaction from './utils/createTransferTransaction';
import { PublicKey } from '@solana/web3.js';
import sendTransaction from './utils/sendTransaction';



window.Buffer = window.Buffer || require("buffer").Buffer;


const App = () => {
  const { wallet, error: werr } = useWallet();
  const { connection, error: cerr } = useConnection();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  const handleTransaction = async () => {
      try {
        const receiver = new PublicKey('EsPxEpdzie7F9fFQPpCQyxV7zEJPAjrVSASo7kAhNyov');
        setLoading(true);
        
        const transaction = await createTransferTransaction(wallet!.publicKey!, receiver, 1_000_000, connection!);
        const signedTransaction = await wallet!.signTransaction(transaction);
        
        await sendTransaction(wallet!.publicKey!, signedTransaction);
        setError(null); // Сбрасываем ошибку после успешной транзакции
      } catch (err: any) {
        setError(`Error while processing transaction: ${err.message}`);
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {
    if (wallet && connection) {
      setLoading(false);
    }
  }, [wallet, connection]);

  return (
    <div style={{ maxWidth: '400px', margin: 'auto', textAlign: 'center', padding: '20px' }}>
      <h1>Wallet Transaction</h1>
      {werr && <p style={{ color: 'red' }}>Wallet Error: {werr}</p>}
      {cerr && <p style={{ color: 'red' }}>Connection Error: {cerr}</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <button onClick={() => handleTransaction()} disabled={loading}>
        {loading ? 'Processing...' : 'Send 1,000,000 Lamports'}
      </button>
    </div>
  );
};

export default App;