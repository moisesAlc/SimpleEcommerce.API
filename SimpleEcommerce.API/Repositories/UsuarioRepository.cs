using Dapper;
using SimpleEcommerce.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SimpleEcommerce.API.Repositories
{

	public class UsuarioRepository : IUsuarioRepository
	{
		private IDbConnection _connection;

		public UsuarioRepository()
		{
			_connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleEcommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
		}

		// não vai fazer JOIN aqui por enquanto, só qdo busca por id
		public List<Usuario> Get()
		{
			return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();
		}


		public Usuario Get(int id)
		{
			return _connection.Query<Usuario, Contato, Usuario>(
				// query com 3 objetos (def no ctor)
				"SELECT * FROM Usuarios as U LEFT JOIN Contatos C ON C.UsuarioId = U.Id WHERE U.Id = @Id;",
				/* +1 param -> função anônima que vai mapear,
				 * para cada linha da query acima, essa f. anonima será executada */
				(usuario, contato) =>
				{
					usuario.Contato = contato; // vai adicionar as infos de contato com as infos só de usuario (sem as infos de contato)
					return usuario;
				},
				new { Id = id }
				).SingleOrDefault();
		}

		public void Insert(Usuario usuario)
		{
			_connection.Open();

			IDbTransaction transaction = _connection.BeginTransaction();

			try
			{
				string sqlUsuario =
				"INSERT INTO Usuarios (Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro) " +
				"VALUES " + "(@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro)" +
				";" +
				"SELECT CAST( SCOPE_IDENTITY() AS INT);"; // pega o id do usuário inserido
				usuario.Id = _connection.Query<int>(sqlUsuario, usuario, transaction).Single();

				if (usuario.Contato != null)
				{
					usuario.Contato.UsuarioId = usuario.Id;
					string sqlContato =
						"INSERT INTO Contatos (UsuarioId, Telefone, Celular) " +
						"VALUES (@UsuarioId, @Telefone, @Celular)" +
						";" +
						"SELECT CAST( SCOPE_IDENTITY() AS INT);"; // pega o id do contato inserido
					usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
				}

				transaction.Commit();
			}
			catch (Exception e)
			{
				try
				{
					transaction.Rollback();
				}
				catch (Exception)
				{
					//@TODO qual é a melhor forma de tratar?
				}

			}
			finally
			{
				_connection.Close();
			}

		}

		public void Update(Usuario usuario)
		{
			_connection.Open();
			var transaction = _connection.BeginTransaction();

			try
			{
				string sqlUsuario =
					"UPDATE Usuarios SET " +
					"Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, " +
					"CPF = @CPF, NomeMae = @NomeMae, " +
					"SituacaoCadastro = @SituacaoCadastro, " +
					"DataCadastro = @DataCadastro " +
					"WHERE Id = @Id;";
				_connection.Execute(sqlUsuario, usuario, transaction);

				if (usuario.Contato != null)
				{
					string sqlContato =
					"UPDATE Contatos SET " +
					"UsuarioId = @UsuarioId, " +
					"Telefone = @Telefone, " +
					"Celular = @Celular " +
					"WHERE Id = @Id";
					_connection.Execute(sqlContato, usuario.Contato, transaction);

					transaction.Commit();
				}
			}
			catch (Exception)
			{
				try
				{
					transaction.Rollback();
				}
				catch (Exception)
				{
					//@TODO qual é a melhor forma de tratar?
				}
			}
			finally
			{
				_connection.Close();
			}
		}

		public void Delete(int id)
		{
			_connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
		}

	}
}