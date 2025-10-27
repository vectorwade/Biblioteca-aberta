import React, {useState} from 'react'

export default function Search({selectedUser}){
  const [q,setQ]=useState('')
  const [items,setItems]=useState([])
  const [detail, setDetail] = useState(null)

  async function doSearch(){
    if(!q) return
    const headers = {}
    if(selectedUser) headers['X-User-Id'] = selectedUser.id
    const res = await fetch('/api/books/search?q='+encodeURIComponent(q), { headers })
    const data = await res.json()
    setItems(data.items || [])
  }

  async function loadDetails(olid){
    const r = await fetch('/api/books/olid/'+olid)
    const d = await r.json()
    setDetail(d)
  }

  return (
    <section className="card">
      <h2>Buscar livros</h2>
      <div className="row">
        <input value={q} onChange={e=>setQ(e.target.value)} placeholder="Título, autor, ISBN..." />
        <button onClick={doSearch}>Buscar</button>
      </div>
      <div className="results">
        {items.map((it, idx)=> (
          <div className="result" key={idx}>
            {it.coverUrl && <img src={it.coverUrl} alt="capa"/>}
            <div className="meta">
              <h3>{it.title}</h3>
              <div className="authors">{(it.authors||[]).join(', ')}</div>
              <div className="actions">
                {it.olid && <button onClick={()=>loadDetails(it.olid)}>Detalhes</button>}
                {it.olid && <a className="link" href={`/api/books/proxydownload/${it.olid}`}>Download</a>}
                <a className="link" target="_blank" rel="noreferrer" href={`https://www.amazon.com/s?k=${encodeURIComponent(it.title||'')}`}>Comprar</a>
              </div>
            </div>
          </div>
        ))}
      </div>

      {detail && (
        <div className="modal">
          <div className="modalContent">
            <button className="close" onClick={()=>setDetail(null)}>X</button>
            <h2>{detail.title}</h2>
            {detail.cover?.medium && <img src={detail.cover.medium} alt="capa"/>}
            <div className="desc">{(typeof detail.description === 'string') ? detail.description : (detail.description?.value || '')}</div>
            <div><a className="link" href={`/api/books/proxydownload/${detail.identifiers?.openlibrary?.[0] || detail.key?.split('/').pop()}`}>Download</a></div>
            <div><a className="link" target="_blank" rel="noreferrer" href={`https://www.amazon.com/s?k=${encodeURIComponent(detail.title||'')}`}>Comprar</a></div>
          </div>
        </div>
      )}
    </section>
  )
}
