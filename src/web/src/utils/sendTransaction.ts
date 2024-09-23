import { PublicKey } from "@solana/web3.js";
import { TransactionOrVersionedTransaction } from "@solflare-wallet/sdk/lib/cjs/types";

async function sendTransaction(publicKey: PublicKey, signed: TransactionOrVersionedTransaction) {  
    const options = {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': publicKey.toString()
        },
        body: JSON.stringify({ 
            base64TransactionData: Buffer.from(signed.serialize()).toString('base64') 
        }),
    };

    const response = await fetch('http://localhost:8080/api/replenishment/transaction?api-version=1.0', options);

    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response;
}

export default sendTransaction;