            string header;
            ChunkList selectedChunks = ChunkList.GetSelectedChunks(text, selectionPosition, selectionLength, staged, out header);
            if (body != null && "true".Equals(module.GetEffectiveSetting("core.autocrlf"), StringComparison.InvariantCultureIgnoreCase))
        public static byte[] GetSelectedLinesAsPatch(GitModule module, string text, int selectionPosition, int selectionLength, bool staged, Encoding fileContentEncoding)
            string header;
            ChunkList selectedChunks = ChunkList.GetSelectedChunks(text, selectionPosition, selectionLength, staged, out header);
            if (selectedChunks == null)
                return null;
            string body = selectedChunks.ToStagePatch(staged);
            if (header == null || body == null)
                return null;
            else
                return GetPatchBytes(header, body, fileContentEncoding);
        }
        public static byte[] GetSelectedLinesAsNewPatch(GitModule module, string newFileName, string text, int selectionPosition, int selectionLength, Encoding fileContentEncoding)
        {
            StringBuilder sb = new StringBuilder();
            string fileMode = "100000";//given fake mode to satisfy patch format, git will override this
            sb.Append(string.Format("diff --git a/{0} b/{0}", newFileName));
            sb.Append("\n");
            sb.Append("new file mode " + fileMode);
            sb.Append("\n");
            sb.Append("index 0000000..0000000");
            sb.Append("\n");
            sb.Append("--- /dev/null");
            sb.Append("\n");
            sb.Append("+++ b/" + newFileName);
            sb.Append("\n");

            string header = sb.ToString();

            ChunkList selectedChunks = ChunkList.FromNewFile(module, text, selectionPosition, selectionLength);

            if (selectedChunks == null)
                return null;
            
            string body = selectedChunks.ToStagePatch(false);
            if (header == null || body == null)
                return null;
            else
                return GetPatchBytes(header, body, fileContentEncoding);
        }
        public static byte[] GetPatchBytes(string header, string body, Encoding fileContentEncoding)
        {
            byte[] bb = EncodingHelper.ConvertTo(fileContentEncoding, body);
            return result;        

        public string ToStagePatch(ref int addedCount, ref int removedCount, ref bool wereSelectedLines, bool staged)
        {
            string diff = null;
            string removePart = null;
            string addPart = null;
            string prePart = null;
            string postPart = null;
            bool inPostPart = false;
            bool selectedLastLine = false;
            addedCount += PreContext.Count + PostContext.Count;
            removedCount += PreContext.Count + PostContext.Count;

            foreach (PatchLine line in PreContext)
                diff = diff.Combine("\n", line.Text);

            for (int i = 0; i < RemovedLines.Count; i++)
            {
                PatchLine removedLine = RemovedLines[i];
                selectedLastLine = removedLine.Selected;
                if (removedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    removePart = removePart.Combine("\n", removedLine.Text);
                    removedCount++;
                }
                else if (!staged)
                {
                    if (inPostPart)
                        removePart = removePart.Combine("\n", " " + removedLine.Text.Substring(1));
                    else
                        prePart = prePart.Combine("\n", " " + removedLine.Text.Substring(1));
                    addedCount++;
                    removedCount++;
                }
            }

            bool selectedLastRemovedLine = selectedLastLine;

            for (int i = 0; i < AddedLines.Count; i++)
            {
                PatchLine addedLine = AddedLines[i];
                selectedLastLine = addedLine.Selected;
                if (addedLine.Selected)
                {
                    wereSelectedLines = true;
                    inPostPart = true;
                    addPart = addPart.Combine("\n", addedLine.Text);
                    addedCount++;
                }

                else if (staged)
                {
                    if (inPostPart)
                        postPart = postPart.Combine("\n", " " + addedLine.Text.Substring(1));
                    else
                        prePart = prePart.Combine("\n", " " + addedLine.Text.Substring(1));
                    addedCount++;
                    removedCount++;
                }

            }

            diff = diff.Combine("\n", prePart);
            diff = diff.Combine("\n", removePart);
            if (PostContext.Count == 0 && (!staged || selectedLastRemovedLine))
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            diff = diff.Combine("\n", addPart);
            diff = diff.Combine("\n", postPart);
            foreach (PatchLine line in PostContext)
                diff = diff.Combine("\n", line.Text);
            //stage no new line at the end only if last +- line is selected 
            if (PostContext.Count == 0 && (selectedLastLine || staged))
                diff = diff.Combine("\n", IsNoNewLineAtTheEnd);
            if (PostContext.Count > 0)
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);

            return diff;
        }

                    addPart = addPart.Combine("\n", "+" + removedLine.Text.Substring(1));
            if (PostContext.Count == 0)
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
            if (PostContext.Count == 0)
                diff = diff.Combine("\n", IsNoNewLineAtTheEnd);
            else
                diff = diff.Combine("\n", WasNoNewLineAtTheEnd);
    internal delegate string SubChunkToPatchFnc(SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines);

                            if (result.CurrentSubChunk.AddedLines.Count > 0 && result.CurrentSubChunk.PostContext.Count == 0)
        public static Chunk FromNewFile(GitModule module, string fileText, int selectionPosition, int selectionLength)
        {
            Chunk result = new Chunk();
            result.StartLine = 0;
            int currentPos = 0;
            string gitEol = module.GetEffectiveSetting("core.eol");
            string eol;
            if ("crlf".Equals(gitEol))
                eol = "\r\n";
            else if ("native".Equals(gitEol))
                eol = Environment.NewLine;
            else
                eol = "\n";            

            int eolLength = eol.Length;

            string[] lines = fileText.Split(new string[] { eol }, StringSplitOptions.None);
            int i = 0;

            while (i < lines.Length)
            {
                string line = lines[i];
                PatchLine patchLine = new PatchLine()
                {
                    Text = "+" + line
                };
                //do not refactor, there are no breakpoints condition in VS Experss
                if (currentPos <= selectionPosition + selectionLength && currentPos + line.Length >= selectionPosition)
                    patchLine.Selected = true;

                if (i == lines.Length - 1)
                {
                    if (!line.Equals(string.Empty))
                    {
                        result.CurrentSubChunk.IsNoNewLineAtTheEnd = "\\ No newline at end of file";
                        result.AddDiffLine(patchLine, false);
                    }
                }
                else
                    result.AddDiffLine(patchLine, false);

                currentPos += line.Length + eolLength;
                i++;
            }
            return result;
        }


        public string ToPatch(SubChunkToPatchFnc subChunkToPatch)
                string subDiff = subChunkToPatch(subChunk, ref addedCount, ref removedCount, ref wereSelectedLines);
        public static ChunkList GetSelectedChunks(string text, int selectionPosition, int selectionLength, bool staged, out string header)
        {
            header = null;
            //When there is no patch, return nothing
            if (string.IsNullOrEmpty(text))
                return null;

            // Divide diff into header and patch
            int patchPos = text.IndexOf("@@");
            if (patchPos < 1)
                return null;

            header = text.Substring(0, patchPos);
            string diff = text.Substring(patchPos - 1);

            string[] chunks = diff.Split(new string[] { "\n@@" }, StringSplitOptions.RemoveEmptyEntries);
            ChunkList selectedChunks = new ChunkList();
            int i = 0;
            int currentPos = patchPos - 1;

            while (i < chunks.Length && currentPos <= selectionPosition + selectionLength)
            {
                string chunkStr = chunks[i];
                currentPos += 3;
                //if selection intersects with chunsk
                if (currentPos + chunkStr.Length >= selectionPosition)
                {
                    Chunk chunk = Chunk.ParseChunk(chunkStr, currentPos, selectionPosition, selectionLength);
                    if (chunk != null)
                        selectedChunks.Add(chunk);
                }
                currentPos += chunkStr.Length;
                i++;
            }

            return selectedChunks;
        }

        public static ChunkList FromNewFile(GitModule module, string text, int selectionPosition, int selectionLength)
        {
            Chunk chunk = Chunk.FromNewFile(module, text, selectionPosition, selectionLength);
            ChunkList result = new ChunkList();
            result.Add(chunk);
            return result;
        }

        {
            SubChunkToPatchFnc subChunkToPatch = (SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines) =>
                {
                    return subChunk.ToResetUnstagedLinesPatch(ref addedCount, ref removedCount, ref wereSelectedLines);
                };

            return ToPatch(subChunkToPatch);
        }

        public string ToStagePatch(bool staged)
        {
            SubChunkToPatchFnc subChunkToPatch = (SubChunk subChunk, ref int addedCount, ref int removedCount, ref bool wereSelectedLines) =>
            {
                return subChunk.ToStagePatch(ref addedCount, ref removedCount, ref wereSelectedLines, staged);
            };

            return ToPatch(subChunkToPatch);
        }

        protected string ToPatch(SubChunkToPatchFnc subChunkToPatch)
                result = result.Combine("\n", chunk.ToPatch(subChunkToPatch));
                result = result.Combine("\n", Application.ProductName + " " + Settings.GitExtensionsVersionString);
        
    