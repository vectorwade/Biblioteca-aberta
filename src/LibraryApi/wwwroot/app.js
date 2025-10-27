const api = {
  users: '/api/users',
  books: '/api/books'
};

async function fetchJson(url, opts){
  const res = await fetch(url, opts);
  return res.json();
}

async function loadUsers(){
  const users = await fetchJson(api.users);
  const sel = document.getElementById('userSelect');
  sel.innerHTML = '<option value="">-- selecione usuário --</option>' + users.map(u=>`<option value="${u.id}">${u.displayName || u.username}</option>`).join('');
}

document.getElementById('btnRegister').addEventListener('click', async ()=>{
  const username = document.getElementById('username').value;
  const display = document.getElementById('displayname').value;
  if(!username) return alert('username required');
  const user = await fetchJson(api.users, {method:'POST', headers:{'content-type':'application/json'}, body:JSON.stringify({username, displayName:display})});
  await loadUsers();
  document.getElementById('username').value=''; document.getElementById('displayname').value='';
  alert('Usuário criado: '+user.username);
});

document.getElementById('btnSearch').addEventListener('click', async ()=>{
  const q = document.getElementById('q').value;
  if(!q) return;
  const userId = document.getElementById('userSelect').value;
  const headers = {};
  if(userId) headers['X-User-Id'] = userId;
  const res = await fetch('/api/books/search?q='+encodeURIComponent(q), {headers});
  const data = await res.json();
  renderResults(data, q, userId);
  if(userId) loadHistory(userId);
}
);

function renderResults(payload, q, userId){
  const results = payload.result;
  const amazon = payload.amazonLink;
  const container = document.getElementById('results');
  if(!results || (results.docs && results.docs.length==0)){
    container.innerHTML = `<p>Nenhum resultado. Comprar: <a target="_blank" href="${amazon}">${amazon}</a></p>`;
    return;
  }
  const docs = results.docs || [];
  container.innerHTML = docs.map(d => {
    const authors = (d.authorName||[]).join(', ');
    const olid = d.coverEditionKey || (d.editionKey && d.editionKey[0]) || d.key;
    const cover = olid ? `https://covers.openlibrary.org/b/olid/${olid}-M.jpg` : '';
    const addBtn = userId ? `<button onclick="addWishlist(${userId},'${olid}','${escape(d.title||'')}')">Adicionar à wishlist</button>` : '';
    return `<div class="row" style="align-items:center"><img src="${cover}" onerror="this.style.display='none'"/><div style="flex:1"><b>${d.title}</b><div>${authors}</div>${addBtn}</div></div>`;
  }).join('<hr/>');
}

window.addWishlist = async function(userId, olid, title){
  title = decodeURIComponent(title);
  await fetch(`/api/users/${userId}/wishlist`, {method:'POST', headers:{'content-type':'application/json'}, body:JSON.stringify({openLibraryId:olid, title})});
  alert('Adicionado à wishlist');
  loadWishlist(userId);
}

async function loadWishlist(userId){
  if(!userId) return;
  const list = await fetchJson(`/api/users/${userId}/wishlist`);
  document.getElementById('wishlist').innerHTML = list.map(w=>`<div>${w.title || w.openLibraryId} <small>${new Date(w.addedAt).toLocaleString()}</small></div>`).join('');
}

async function loadHistory(userId){
  if(!userId) return;
  const list = await fetchJson(`/api/users/${userId}/searchHistory`);
  document.getElementById('history').innerHTML = list.map(h=>`<div>${h.query} <small>${new Date(h.timestamp).toLocaleString()}</small></div>`).join('');
}

async function loadRanking(){
  const rk = await fetchJson('/api/users/ranking');
  document.getElementById('ranking').innerHTML = rk.map(r=>`<div>${r.borrower || '(unknown)'} — ${r.loans} empréstimos</div>`).join('');
}

document.getElementById('userSelect').addEventListener('change', (e)=>{
  const uid = e.target.value;
  loadWishlist(uid);
  loadHistory(uid);
});

loadUsers(); loadRanking();

document.getElementById('btnSearchFree').addEventListener('click', async ()=>{
  const q = document.getElementById('qfree').value;
  if(!q) return;
  const res = await fetch('/api/books/free?q='+encodeURIComponent(q));
  const data = await res.json();
  const container = document.getElementById('freeResults');
  if(!data.items || data.items.length==0){
    container.innerHTML = '<p>Nenhum livro gratuito encontrado.</p>';
    return;
  }
  container.innerHTML = data.items.map(b=>{
    const authors = (b.authors||[]).join(', ');
    const cover = b.cover;
    return `<div class="row" style="align-items:center"><img src="${cover}" onerror="this.style.display='none'"/><div style="flex:1"><b>${b.title}</b><div>${authors}</div><div>${b.description||''}</div><a target="_blank" href="/api/books/proxydownload/${b.olid}"><button>Download (proxy)</button></a></div></div>`;
  }).join('<hr/>');
});
