import { Connection, PublicKey, SystemProgram, Transaction } from "@solana/web3.js";

async function createTransferTransaction(fromPubkey: PublicKey, toPubkey: PublicKey, lamports: number, connection: Connection) {
    // Создаем новую транзакцию и добавляем инструкцию перевода
    const transaction = new Transaction().add(
        SystemProgram.transfer({
            fromPubkey: fromPubkey,
            toPubkey: toPubkey,
            lamports: lamports
        })
    );

    // Устанавливаем плательщика комиссии
    transaction.feePayer = new PublicKey(fromPubkey);

    // Получаем последний блокхеш и устанавливаем его в транзакцию
    const { blockhash } = await connection.getLatestBlockhash();
    transaction.recentBlockhash = blockhash;

    return transaction;
}

export default createTransferTransaction;