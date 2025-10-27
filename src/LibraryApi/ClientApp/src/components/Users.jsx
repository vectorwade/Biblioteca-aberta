import React, {useState, useEffect} from 'react'

export default function Users({onSelect, selected}){
  const [users, setUsers] = useState([])
  const [username, setUsername] = useState('')
  const [display, setDisplay] = useState('')

  useEffect(()=>{ load() }, [])
  async function load(){
    const res = await fetch('/api/users')
    const data = await res.json()
    setUsers(data)
  }

  async function register(){
    if(!username) return alert('username required')
    const res = await fetch('/api/users', {method:'POST', headers:{'content-type':'application/json'}, body: JSON.stringify({username, displayName:display})})
    const u = await res.json()
    setUsername(''); setDisplay('');
    await load();
    onSelect(u)
  }

  return (
    <div className="users">
      <div className="register">
        <input placeholder="username" value={username} onChange={e=>setUsername(e.target.value)} />
        <input placeholder="display name" value={display} onChange={e=>setDisplay(e.target.value)} />
        <button onClick={register}>Registrar</button>
      </div>
      <div className="select">
        <select onChange={e=> onSelect(users.find(u=>u.id==e.target.value)) } value={selected?.id || ''}>
          <option value="">-- selecione usuário --</option>
          {users.map(u=> <option key={u.id} value={u.id}>{u.displayName || u.username}</option>)}
        </select>
      </div>
    </div>
  )
}
