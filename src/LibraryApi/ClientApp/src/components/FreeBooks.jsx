import React, {useState} from 'react'

export default function FreeBooks({selectedUser}){
  const [q,setQ]=useState('')
  const [items,setItems]=useState([])

  async function doSearch(){
    if(!q) return
    const res = await fetch('/api/books/free?q='+encodeURIComponent(q))
    const data = await res.json()
    setItems(data.items || [])
  }

  return (
    <section className="card">
      <h2>Livros gratuitos</h2>
      <div className="row">
        <input value={q} onChange={e=>setQ(e.target.value)} placeholder="Buscar livros gratuitos..." />
        <button onClick={doSearch}>Buscar gratuitos</button>
      </div>
      <div className="results">
        {items.map((b, idx)=> (
          <div className="result" key={idx}>
            {b.cover && <img src={b.cover} alt="capa"/>}
            <div className="meta">
              <h3>{b.title}</h3>
              <div className="authors">{(b.authors||[]).join(', ')}</div>
              <div className="desc">{b.description}</div>
              <div className="actions">
                <a className="link" href={`/api/books/proxydownload/${b.olid}`}>Download</a>
                <a className="link" target="_blank" rel="noreferrer" href={`https://www.amazon.com/s?k=${encodeURIComponent(b.title||'')}`}>Comprar</a>
              </div>
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
