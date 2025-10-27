import React, {useState, useEffect} from 'react'
import Search from './components/Search'
import FreeBooks from './components/FreeBooks'
import Users from './components/Users'
import './index.css'

export default function App(){
  const [user, setUser] = useState(null)

  useEffect(()=>{
    // try to preserve selected user in localStorage
    const raw = localStorage.getItem('ba_user')
    if(raw) setUser(JSON.parse(raw))
  },[])

  return (
    <div className="container">
      <header>
        <h1>Biblioteca Aberta</h1>
        <Users onSelect={u=>{ setUser(u); localStorage.setItem('ba_user', JSON.stringify(u)) }} selected={user} />
      </header>
      <main>
        <Search selectedUser={user} />
        <FreeBooks selectedUser={user} />
      </main>
      <footer>
        <small>Dados via Open Library • Projeto exemplo</small>
      </footer>
    </div>
  )
}
